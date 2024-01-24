using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Data;
using MvcClinic.DTOs;
using MvcClinic.Models;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MvcClinic.Controllers
{
    [Authorize]
    public class SchedulesController : Controller
    {
        private readonly MvcClinicContext _context;

        private readonly UserManager<Employee> _employeeManager;
        private readonly UserManager<Patient> _patientManager;
        private readonly IAuthorizationService _authorizationService;

        public SchedulesController(MvcClinicContext context, UserManager<Employee> employeeManager, UserManager<Patient> patientManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _employeeManager = employeeManager;
            _patientManager = patientManager;
            _authorizationService = authorizationService;
        }

        // GET: Schedules
        [HttpGet("[controller]/index")]
        public async Task<ActionResult<ScheduleListViewDTO>> Index(DateTime? DateFrom, DateTime? DateTo, int? SpecialityId)
        {
            bool isAdmin = false;
            bool isDoctor = false;
            bool isPatient = false;
            List<Schedule> schedules = [];
            List<Speciality> specialities = [];

            if (DateFrom == null)
            {
                DateTime startOfWeek = DateTime.Today.AddDays(-((7 + ((int)DateTime.Today.DayOfWeek) - (int)DayOfWeek.Monday) % 7));
                DateFrom = startOfWeek;
            }
            if (DateTo == null)
            {
                DateTo = DateFrom.Value.AddDays(7);
            }

            if (DateFrom >= DateTo)
            {
                return BadRequest("Wrong dates");
            }

            if ((await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded) {
                isAdmin = true;
            } else if ((await _authorizationService.AuthorizeAsync(User, "DoctorOnly")).Succeeded) {
                isDoctor = true;
            } else if ((await _authorizationService.AuthorizeAsync(User, "PatientOnly")).Succeeded) {
                isPatient = true;
            }
            if (!isAdmin && !isDoctor && !isPatient)
            {
                return Unauthorized();
                //return RedirectToAction("Index", "Home");
            }

            specialities = await _context.Speciality.ToListAsync();
            var email = User.FindFirst(ClaimTypes.Email).Value;
            
            if (isPatient)
            {
                var patient = await _patientManager.FindByEmailAsync(email);
                if(!patient!.Active)
                {
                    return Unauthorized("Patient account not activated");
                    //return RedirectToAction(nameof(AccessDenied));
                }
                schedules = await _context.Schedule.Include(s => s.Doctor)
                    .Include(s => s.Doctor.Specialization).Include(s => s.Patient)
                    .Where(s => DateFrom <= s.Date && s.Date <= DateTo)
                    .Where(s => s.Patient == patient || s.Patient == null)
                    .Where(s => SpecialityId == null || s.Doctor.Specialization.Id == SpecialityId)
                    .Where(s => s.Doctor != null)
                    .OrderBy(s => s.Date).ToListAsync();
                schedules.RemoveAll(s => (s.Patient == null && s.Date < DateTime.Now));
            }

            if (isDoctor)
            {
                var doctor = await _employeeManager.FindByEmailAsync(email);
                schedules = await _context.Schedule.Include(s => s.Patient)
                    .Where(s => (DateFrom <= s.Date && s.Date <= DateTo))
                    .Where(s => s.Doctor == doctor).OrderBy(s => s.Date).ToListAsync();
            }

            if (isAdmin)
            {
                schedules = await _context.Schedule.Include(s => s.Doctor)
                    .Include(s => s.Doctor.Specialization)
                    .Include(s => s.Patient)
                    .Where(s => DateFrom <= s.Date && s.Date <= DateTo)
                    .Where(s => SpecialityId == null || s.Doctor.Specialization.Id == SpecialityId)
                    .OrderBy(s => s.Date).ToListAsync();
            }

            return new ScheduleListViewDTO
            {
                isAdmin = isAdmin,
                isDoctor = isDoctor,
                isPatient = isPatient,
                Schedules = schedules,
                DateFrom = (DateTime)DateFrom,
                DateTo = (DateTime)DateTo,
                SpecialityId = SpecialityId,
                Specalities = specialities
            };
        }

        [HttpGet("[controller]/copyFromLastWeek")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ScheduleCopyListViewDTO>> CopyFromLastWeek()
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-((7 + ((int)DateTime.Today.DayOfWeek) - (int)DayOfWeek.Monday) % 7));
            DateTime endOfWeek = startOfWeek.AddDays(7);
            DateTime endOfNextWeek = startOfWeek.AddDays(14);

            var oldSchedules = await _context.Schedule.Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Include(s => s.Patient)
                .Where(s => (endOfWeek <= s.Date && s.Date <= endOfNextWeek))
                .OrderBy(s => s.Date).ToListAsync();

            var newSchedules = await _context.Schedule.Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Where(s => (startOfWeek <= s.Date && s.Date <= endOfWeek))
                .OrderBy(s => s.Date).ToListAsync();

            newSchedules.ForEach(el => {
                el.Date = el.Date.AddDays(7);
                el.Patient = null;
            });

            List<Schedule> conflictingSchedlues = [];

            newSchedules.RemoveAll(el => {
                if (el.Doctor != null && IsWithin15Minutes(el.Doctor, el.Date, null))
                {
                    conflictingSchedlues.Add(el);
                    return true;
                }
                return false;
            });

            var combinedSchedules = oldSchedules.Concat(newSchedules).OrderBy(el => el.Date).ToList();


            return new ScheduleCopyListViewDTO
            {
                OldSchedules = oldSchedules,
                NewSchedules = newSchedules,
                CombinedSchedules = combinedSchedules,
                ConflictingSchedules = conflictingSchedlues,
                DateFrom = endOfWeek,
                DateTo = endOfNextWeek
            };
        }

        [HttpPost("[controller]/copyFromLastWeek")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CopyFromLastWeek(bool? dummy)
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-((7 + ((int)DateTime.Today.DayOfWeek) - (int)DayOfWeek.Monday) % 7));
            DateTime endOfWeek = startOfWeek.AddDays(7);
            DateTime endOfNextWeek = startOfWeek.AddDays(14);

            var newSchedules = await _context.Schedule
                .Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Where(s => (startOfWeek <= s.Date && s.Date <= endOfWeek))
                .OrderBy(s => s.Date).ToListAsync();


            newSchedules.ForEach(el => { 
                _context.Entry(el).State = EntityState.Detached;
                el.Date = el.Date.AddDays(7); 
                el.Id = 0;
                el.Patient = null;
                el.Description = null;
            });

            newSchedules.RemoveAll(el => {
                if (el.Doctor != null && IsWithin15Minutes(el.Doctor, el.Date, null))
                {
                    return true;
                }
                return false;
            });


            await _context.AddRangeAsync(newSchedules);
            await _context.SaveChangesAsync();

            //return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = startOfWeek, DateTo = endOfWeek});
            return Ok();
        }

        [HttpGet("[controller]/generateReport")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ScheduleReportViewDTO>> GenerateReport(DateOnly dateFrom, DateOnly dateTo)
        {
            var doctors = await _context.Employee.Include(d => d.Specialization).ToListAsync();
            DateTime dateTimeFrom = dateFrom.ToDateTime(TimeOnly.MinValue);
            DateTime dateTimeTo = dateTo.ToDateTime(TimeOnly.MinValue);


            List<ReportEntry> reportEntries = new List<ReportEntry>();

            if(dateTimeFrom >= dateTimeTo)
            {
                //TempData["WrongDates"] = true;
                return BadRequest("Wrong dates");
                //return View(new ScheduleReportViewModel { DateFrom=dateFrom, DateTo=dateTo, ReportEntries=reportEntries });
            }

            doctors.ForEach(d =>
            {
                int? pastSchedulesNumber = null;
                int? pastSchedulesWithPatientNumber = null;
                int? futureScheduleNumber = null;
                int? futureSchedulesWithPatientNumber = null;
                DateTime upperBoundDate = dateTimeTo < DateTime.Now ? dateTimeTo : DateTime.Now;
                DateTime lowerBoundDate = dateTimeFrom > DateTime.Now ? dateTimeFrom : DateTime.Now;
                if (dateTimeFrom <= DateTime.Now)
                {
                    pastSchedulesNumber = _context.Schedule
                        .Where(s => s.Doctor == d)
                        .Where(s => s.Date <= upperBoundDate && dateTimeFrom <= s.Date)
                        .Count();
                    pastSchedulesWithPatientNumber = _context.Schedule
                        .Where(s => s.Doctor == d)
                        .Where(s => s.Date <= upperBoundDate && dateTimeFrom <= s.Date)
                        .Where(s => s.Description != null && s.Patient != null)
                        .Count();
                }
                
                if (DateTime.Now <= dateTimeTo)
                {
                    futureScheduleNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => lowerBoundDate < s.Date && s.Date <= dateTimeTo)
                            .Count();
                    futureSchedulesWithPatientNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => lowerBoundDate < s.Date && s.Date <= dateTimeTo)
                            .Where(s => s.Patient != null)
                            .Count();
                }
                reportEntries.Add(new ReportEntry
                {
                    DoctorName = d.FirstName + " " + d.Surname,
                    DoctorSpecialization = d.Specialization?.Name,
                    PastSchedulesNumber = pastSchedulesNumber,
                    PastSchedulesWithPatientNumber = pastSchedulesWithPatientNumber,
                    FutureSchedulesNumber = futureScheduleNumber,
                    FutureSchedulesWithPatientNumber = futureSchedulesWithPatientNumber,
                }
                );
            });

            return new ScheduleReportViewDTO
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportEntries = reportEntries.OrderBy(re => re.DoctorSpecialization).ToList(),
            };
        }

        //[HttpGet]
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}

        // GET: Schedules/Create
        [HttpGet("[controller]/create")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ScheduleCreateOrEditViewModel>> Create()
        {
            return new ScheduleCreateOrEditViewModel { Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync()};
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/create")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Create([Bind("Date,DoctorId,Description")] ScheduleCreateOrEditViewModel scheduleCreateViewModel)
        {
            if (scheduleCreateViewModel.Date == null)
            {
                return BadRequest();
            }
            Schedule schedule = new Schedule { Date=(DateTime) scheduleCreateViewModel.Date, Description = scheduleCreateViewModel.Description};
            if (!System.String.IsNullOrEmpty(scheduleCreateViewModel.DoctorId))
            {
                var doctor = await _context.Employee.FindAsync(scheduleCreateViewModel.DoctorId);
                if (doctor == null)
                {
                    return NotFound("Doctor not found");
                }
                schedule.Doctor = doctor;
                if (IsWithin15Minutes(doctor, (DateTime) scheduleCreateViewModel.Date, null))
                {
                    //TempData["Conflict"] = true;
                    //return RedirectToAction("Create", "Schedules");
                    //scheduleCreateViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
                    //return View(scheduleCreateViewModel);
                    return BadRequest("Scheduling conflict");
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return Ok();
            }
            //scheduleCreateViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
            return BadRequest("Invalid model state");
            //return View(scheduleCreateViewModel);
        }

        // GET: Schedules/Details/5
        [HttpGet("[controller]/details")]
        public async Task<ActionResult<Schedule>> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule
                .Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Include(s => s.Patient)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }
            var email = User.FindFirst(ClaimTypes.Email).Value;

            if ((await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded)
            {
                
            }
            else if ((await _authorizationService.AuthorizeAsync(User, "DoctorOnly")).Succeeded)
            {
                var doctor = await _employeeManager.FindByEmailAsync(email);
                if (schedule.Doctor == null || doctor.Id != schedule.Doctor.Id)
                {
                    return Unauthorized("Only admin can access other doctor's schedules.");
                }
            }
            else if ((await _authorizationService.AuthorizeAsync(User, "PatientOnly")).Succeeded)
            {
                var patient = await _patientManager.FindByEmailAsync(email);
                if (!patient.Active)
                {
                    return Unauthorized("Patient not authorized");
                    //return RedirectToAction(nameof(AccessDenied));
                }
                if ((schedule.Patient == null && schedule.Date < DateTime.Now) || (schedule.Patient != null && patient.Id != schedule.Patient.Id))
                {
                    return Unauthorized("Only admin can access other patient's schedules.");
                }
                if (schedule.Doctor == null)
                {
                    return Unauthorized("Schedule inaccessible.");
                }
            } else
            {
                return Unauthorized();
            }

            return schedule;
        }

        // GET: Schedules/Edit/5
        [HttpGet("[controller]/edit")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<ActionResult<ScheduleCreateOrEditViewModel>> Edit(int? id)
        {
            bool isDoctor = false;
            if (!(await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded)
            {
                isDoctor = true;
            }

            if (id == null)
            {
                return NotFound();
            }
            var schedule = await _context.Schedule.Include(s => s.Doctor).Include(s => s.Patient).FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }

            List<Employee> doctors = [];

            if (isDoctor)
            {
                var email = User.FindFirst(ClaimTypes.Email).Value;
                var doctor = await _employeeManager.FindByEmailAsync(email);

                if (schedule.Doctor != doctor )
                {
                    return Unauthorized("Can't edit other Doctor's schedules");
                }

                doctors.Add(doctor!);
            } else
            {
                doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
            }


            var model = new ScheduleCreateOrEditViewModel
            {
                Date = schedule.Date,
                Id = schedule.Id,
                Doctors = doctors,
                Description = schedule.Description,
                IsDoctor = isDoctor,
                ConcurrencyStamp = schedule.ConcurrencyStamp,
            };
            if(schedule.Patient != null)
            {
                model.Patient = schedule.Patient.FirstName + " " + schedule.Patient.Surname + " [" + schedule.Patient.Email + "]";
                model.PatientId = schedule.Patient.Id;
            }
            if(schedule.Doctor != null) { 
                model.DoctorId = schedule.Doctor.Id;
            }
            return model;
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/edit")]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<ActionResult> Edit(int id, [Bind("Id,Date,DoctorId,Description,ConcurrencyStamp")] ScheduleCreateOrEditViewModel scheduleEditViewModel)
        {
            if (id != scheduleEditViewModel.Id)
            {
                return NotFound();
            }

            if (scheduleEditViewModel.Date == null)
            {
                return BadRequest();
            }

            var schedule = await _context.Schedule.Include(s => s.Patient).Include(s => s.Doctor).FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            bool isDoctor = !(await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;
            Employee? doctor = new Employee { };
            if (schedule.Date < DateTime.Now || isDoctor)
            {
                string message_prefix = "Can't edit past schedule's";
                if (isDoctor)
                {
                    var email = User.FindFirst(ClaimTypes.Email).Value;
                    doctor = await _employeeManager.FindByEmailAsync(email);

                    if (schedule.Doctor != doctor)
                    {
                        return Unauthorized("Can't edit other Doctor's schedules");
                    }
                    message_prefix = "Only admin can edit";
                }

                if (schedule.Date != scheduleEditViewModel.Date && schedule.ConcurrencyStamp == scheduleEditViewModel.ConcurrencyStamp)
                {
                    return BadRequest(message_prefix + " Date");
                }

                if ((scheduleEditViewModel.DoctorId != null && schedule.Doctor == null) ||
                    (scheduleEditViewModel.DoctorId == null && schedule.Doctor != null) ||
                    (scheduleEditViewModel.DoctorId != schedule.Doctor.Id))
                {
                    return BadRequest(message_prefix + " Doctor");
                }
            }

            if (scheduleEditViewModel.DoctorId == null)
            {
                schedule.Doctor = null;
            }
            else
            {
                doctor = await _context.Employee.FindAsync(scheduleEditViewModel.DoctorId);
                if (doctor == null)
                {
                    return NotFound();
                }
                schedule.Doctor = doctor;
                if (IsWithin15Minutes(doctor, (DateTime) scheduleEditViewModel.Date, schedule.Id))
                {
                    //TempData["Conflict"] = true;
                    scheduleEditViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
                    //return View(scheduleEditViewModel);
                    return BadRequest("Scheduling conflict");
                }
            }

            schedule.Description = scheduleEditViewModel.Description;
            if (scheduleEditViewModel.Date == null)
            {
                return BadRequest();
            } else
            {
                schedule.Date = (DateTime) scheduleEditViewModel.Date;
            }

            _context.Entry(schedule).OriginalValues["ConcurrencyStamp"] = scheduleEditViewModel.ConcurrencyStamp;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    schedule.ConcurrencyStamp = Guid.NewGuid();
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return BadRequest("Concurrency exception");
                        //await _context.Entry(schedule).ReloadAsync();
                        //TempData["ConcurrencyException"] = true;
                        //scheduleEditViewModel.Date = schedule.Date;
                        //scheduleEditViewModel.Description = schedule.Description;

                        //if (schedule.Patient != null)
                        //{
                        //    scheduleEditViewModel.Patient = schedule.Patient.FirstName + " " + schedule.Patient.Surname + " [" + schedule.Patient.Email + "]";
                        //    scheduleEditViewModel.PatientId = schedule.Patient.Id;
                        //}
                        //if (schedule.Doctor != null)
                        //{
                        //    scheduleEditViewModel.DoctorId = schedule.Doctor.Id;
                        //}
                        //if (isDoctor && schedule.Doctor != doctor)
                        //{
                        //    return Unauthorized();
                        //}
                    }
                }
            }
            //ModelState.Clear();
            //scheduleEditViewModel.ConcurrencyStamp = schedule.ConcurrencyStamp;

            //if (!isDoctor)
            //{
            //    scheduleEditViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
            //} else
            //{
            //    scheduleEditViewModel.Doctors = new List<Employee> {doctor};
            //}
            //scheduleEditViewModel.IsDoctor = isDoctor;
            //return View(scheduleEditViewModel);
            return Ok();
        }

        // POST: Schedules/Book/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/book")]
        [Authorize(Policy = "PatientOnly")]
        public async Task<ActionResult> Book(int id, Guid concurrencyStamp, DateTime? dateFrom, DateTime? dateTo, int? specialityId)
        {
            var schedule = await _context.Schedule.Include(s => s.Doctor).Include(s => s.Patient).FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.ConcurrencyStamp != concurrencyStamp)
            {
                //TempData["ConcurrencyException"] = true;
                //return RedirectToAction(nameof(Index), "Schedules", new { DateFrom = dateFrom, DateTo = dateTo, SpecialityId = specialityId });
                return BadRequest("Concurrency exception");
            }

            if (schedule.Date < DateTime.Now)
            {
                return BadRequest("Can't book or unbook past schedule");
            }

            if (schedule.Doctor == null)
            {
                return BadRequest("Can't book schedule without doctor");
            }
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var patient = await _patientManager.FindByEmailAsync(email);
            if (schedule.Patient == null)
            {
                schedule.Patient = patient;
            } else if (schedule.Patient == patient)
            {
                schedule.Patient = null;
            } else
            {
                return Unauthorized();
            }

            _context.Entry(schedule).OriginalValues["ConcurrencyStamp"] = concurrencyStamp;

            if (specialityId == 0)
            {
                specialityId = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    schedule.ConcurrencyStamp = Guid.NewGuid();
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        //TempData["ConcurrencyExceptionPatient"] = true;
                        //return RedirectToAction(nameof(Index), "Schedules", new { DateFrom = dateFrom, DateTo = dateTo, SpecialityId=specialityId });
                        return BadRequest("Concurrency exception");
                    }
                }
            }
            return Ok();
            //return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = dateFrom, DateTo=dateTo, SpecialityId=specialityId});
        }

        // GET: Schedules/Delete/5
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("[controller]/delete")]
        public async Task<ActionResult<Schedule>> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var schedule = await _context.Schedule.Include(s => s.Doctor).Include(s => s.Doctor.Specialization)
                .Include(s => s.Patient).FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }

            //if (schedule.Date < DateTime.Now)
            //{
            //    //return RedirectToAction("Details", new {id = id});
            //    return BadRequest("Deletion of past schedules not allowed");
            //}

            return schedule;
            //return View(schedule);
        }

        // POST: Schedules/Delete/5
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("[controller]/delete")]
        public async Task<ActionResult> DeleteConfirmed(int id, Guid concurrencyStamp)
        {
            var schedule = await _context.Schedule.FindAsync(id);

            if (schedule != null)
            {
                _context.Entry(schedule).OriginalValues["ConcurrencyStamp"] = concurrencyStamp;
                if (schedule.Date < DateTime.Now)
                {
                    return BadRequest("Can't delete past schedule");
                }
                try
                {
                    _context.Schedule.Remove(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                    {
                        return BadRequest("Schedule doesn't exist");
                        //TempData["ConcurrencyExceptionAlreadyDeleted"] = true;
                    }
                    else
                    {
                        return BadRequest("Concurrency exception");
                        //TempData["ConcurrencyExceptionDelete"] = true; 
                    }
                }
            }

            return Ok();
            //return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedule.Any(e => e.Id == id);
        }

        private bool IsWithin15Minutes(Employee doctor, DateTime date, int? ScheduleId)
        {
            var visitConflictQuery = _context.Schedule.Where(s => s.Doctor == doctor)
                .Where(s => s.Date < date.AddMinutes(15) && date.AddMinutes(-15) < s.Date);
            if (ScheduleId != null)
            {
                visitConflictQuery = visitConflictQuery.Where(s => s.Id != ScheduleId);
            }
            var visitConflict = visitConflictQuery
                .FirstOrDefault();
            if (visitConflict != null)
            {
                return true;
            }
            return false;
        }
    }
}

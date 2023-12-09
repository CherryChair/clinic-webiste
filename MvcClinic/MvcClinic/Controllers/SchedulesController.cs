using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Data;
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
        public async Task<IActionResult> Index(DateTime? DateFrom, DateTime? DateTo, int? SpecialityId)
        {
            if (DateFrom == null)
            {
                DateTime startOfWeek = DateTime.Today.AddDays(-((7 + ((int)DateTime.Today.DayOfWeek) - (int)DayOfWeek.Monday) % 7));
                DateFrom = startOfWeek;
            }
            if (DateTo == null)
            {
                DateTo = DateFrom.Value.AddDays(7);
            }

            bool isAdmin = false;
            bool isDoctor = false;
            bool isPatient = false;
            if ((await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded) {
                isAdmin = true;
            } else if ((await _authorizationService.AuthorizeAsync(User, "DoctorOnly")).Succeeded) {
                isDoctor = true;
            } else if ((await _authorizationService.AuthorizeAsync(User, "PatientOnly")).Succeeded) {
                isPatient = true;
            }
            var employee = await _employeeManager.GetUserAsync(User);
            if (!isAdmin && !isDoctor && !isPatient)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Schedule>? schedules = null;
            
            if (isPatient)
            {
                var patient = await _patientManager.GetUserAsync(User);
                if(!patient!.Active)
                {
                    return RedirectToAction(nameof(AccessDenied));
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
                var doctor = await _employeeManager.GetUserAsync(User);
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
            
            return View(new ScheduleListViewModel
            {
                isAdmin = isAdmin,
                isDoctor = isDoctor,
                isPatient = isPatient,
                Schedules = schedules,
                DateFrom = (DateTime)DateFrom,
                DateTo = (DateTime)DateTo,
                SpecialityId = SpecialityId,
                Specalities = await _context.Speciality.ToListAsync()
            });
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CopyFromLastWeek()
        {
            DateTime startOfWeek = DateTime.Today.AddDays(-((7 + ((int)DateTime.Today.DayOfWeek) - (int)DayOfWeek.Monday) % 7));
            DateTime endOfWeek = startOfWeek.AddDays(7);
            DateTime endOfNextWeek = startOfWeek.AddDays(14);

            var oldSchedules = await _context.Schedule.Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Where(s => (endOfWeek <= s.Date && s.Date <= endOfNextWeek))
                .OrderBy(s => s.Date).ToListAsync();

            var newSchedules = await _context.Schedule.Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Where(s => (startOfWeek <= s.Date && s.Date <= endOfWeek))
                .OrderBy(s => s.Date).ToListAsync();

            newSchedules.ForEach(el => {
                el.Date = el.Date.AddDays(7);
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


            return View(new ScheduleCopyListViewModel
            {
                OldSchedules = oldSchedules,
                NewSchedules = newSchedules,
                CombinedSchedules = combinedSchedules,
                ConflictingSchedules = conflictingSchedlues,
                DateFrom = endOfWeek,
                DateTo = endOfNextWeek
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CopyFromLastWeek(bool? dummy)
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

            return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = startOfWeek, DateTo = endOfWeek});
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GenerateReport(DateOnly dateFrom, DateOnly dateTo)
        {
            var doctors = await _context.Employee.Include(d => d.Specialization).ToListAsync();
            DateTime dateTimeFrom = dateFrom.ToDateTime(TimeOnly.MinValue);
            DateTime dateTimeTo = dateTo.ToDateTime(TimeOnly.MinValue);

            List<ReportEntry> reportEntries = new List<ReportEntry>();

            doctors.ForEach(d =>
            {
                reportEntries.Add(new ReportEntry
                {
                    DoctorName = d.FirstName + " " + d.Surname,
                    DoctorSpecialization = d.Specialization?.Name,
                    PastSchedulesNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => dateTimeFrom <= s.Date && s.Date < DateTime.Now)
                            .Count(),
                    PastSchedulesWithPatientNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => dateTimeFrom <= s.Date && s.Date < DateTime.Now)
                            .Where(s => s.Description != null && s.Patient != null)
                            .Count(),
                    FutureSchedulesNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => DateTime.Now <= s.Date && s.Date <= dateTimeTo)
                            .Count(),
                    FutureSchedulesWithPatientNumber = _context.Schedule
                            .Where(s => s.Doctor == d)
                            .Where(s => DateTime.Now <= s.Date && s.Date <= dateTimeTo)
                            .Where(s => s.Patient != null)
                            .Count(),
                }
                );
            });

            return View(new ScheduleReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                ReportEntries = reportEntries.OrderBy(re => re.DoctorSpecialization).ToList(),
            });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Schedules/Create
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create()
        {
            return View(new ScheduleCreateOrEditViewModel { Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync()});
        }

        // POST: Schedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([Bind("Date,DoctorId,Description")] ScheduleCreateOrEditViewModel scheduleCreateViewModel)
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
                    TempData["Conflict"] = true;
                    //return RedirectToAction("Create", "Schedules");
                    scheduleCreateViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
                    return View(scheduleCreateViewModel);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            scheduleCreateViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
            return View(scheduleCreateViewModel);
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
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

            if ((await _authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded)
            {
                
            }
            else if ((await _authorizationService.AuthorizeAsync(User, "DoctorOnly")).Succeeded)
            {
                var doctor = await _employeeManager.GetUserAsync(User);
                if (schedule.Doctor == null || doctor.Id != schedule.Doctor.Id)
                {
                    return Unauthorized("Only admin can access other doctor's schedules.");
                }
            }
            else if ((await _authorizationService.AuthorizeAsync(User, "PatientOnly")).Succeeded)
            {
                var patient = await _patientManager.GetUserAsync(User);
                if (!patient.Active)
                {
                    return RedirectToAction(nameof(AccessDenied));
                }
                if ((schedule.Patient == null && schedule.Date < DateTime.Now) || (schedule.Patient != null && patient.Id != schedule.Patient.Id))
                {
                    return Unauthorized("Only admin can access other patient's schedules.");
                }
                if (schedule.Doctor == null)
                {
                    return Unauthorized("Schedule inaccessible.");
                }
            }

            return View(schedule);
        }

        // GET: Schedules/Edit/5
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> Edit(int? id)
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
                var doctor = await _employeeManager.GetUserAsync(User);

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
                IsDoctor = isDoctor
            };
            if(schedule.Patient != null)
            {
                model.Patient = schedule.Patient.FirstName + " " + schedule.Patient.Surname + " [" + schedule.Patient.Email + "]";
                model.PatientId = schedule.Patient.Id;
            }
            if(schedule.Doctor != null) { 
                model.DoctorId = schedule.Doctor.Id;
            }
            return View(model);
        }

        // POST: Schedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,DoctorId,Description")] ScheduleCreateOrEditViewModel scheduleEditViewModel)
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
            if (schedule.Date < DateTime.Now || isDoctor)
            {
                string message_prefix = "Can't edit past schedule's";
                Employee doctor = new Employee { };
                if (isDoctor)
                {
                    doctor = await _employeeManager.GetUserAsync(User);

                    if (schedule.Doctor != doctor)
                    {
                        return Unauthorized("Can't edit other Doctor's schedules");
                    }
                    message_prefix = "Only admin can edit";
                }

                if (schedule.Date != scheduleEditViewModel.Date)
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
                var doctor = await _context.Employee.FindAsync(scheduleEditViewModel.DoctorId);
                if (doctor == null)
                {
                    return NotFound();
                }
                schedule.Doctor = doctor;
                if (IsWithin15Minutes(doctor, (DateTime) scheduleEditViewModel.Date, schedule.Id))
                {
                    TempData["Conflict"] = true;
                    scheduleEditViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
                    return View(scheduleEditViewModel);
                }
            }
            schedule.Description = scheduleEditViewModel.Description;
            if(scheduleEditViewModel.Date == null)
            {
                return BadRequest();
            } else
            {
                schedule.Date = (DateTime) scheduleEditViewModel.Date;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
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
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            scheduleEditViewModel.Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync();
            return View(schedule);
        }

        // POST: Schedules/Book/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Book(int id, DateTime? dateFrom, DateTime? dateTo, int? specialityId)
        {
            var schedule = await _context.Schedule.Include(s => s.Doctor).Include(s => s.Patient).FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Date < DateTime.Now)
            {
                return BadRequest("Can't book or unbook past schedule");
            }

            if (schedule.Doctor == null)
            {
                return BadRequest("Can't book schedule without doctor");
            }

            var patient = await _patientManager.GetUserAsync(User);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
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
                        throw;
                    }
                }
            }
            if (specialityId == 0)
            {
                specialityId = null;
            }
            return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = dateFrom, DateTo=dateTo, SpecialityId=specialityId});
        }

        // GET: Schedules/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
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

            if (schedule.Date < DateTime.Now)
            {
                return RedirectToAction("Details", new {id = id});
            }

            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedule.FindAsync(id);

            if (schedule != null)
            {
                if (schedule.Date < DateTime.Now)
                {
                    return BadRequest("Can't book or unbook past schedule");
                }
                _context.Schedule.Remove(schedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
                visitConflictQuery.Where(s => s.Id != ScheduleId);
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

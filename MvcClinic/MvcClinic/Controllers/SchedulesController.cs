﻿using System;
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
        public async Task<IActionResult> Index(DateTime? DateFrom, DateTime? DateTo)
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
                    .Where(s => (DateFrom <= s.Date && s.Date <= DateTo))
                    .Where(s => (s.Patient == patient) || (s.Patient == null))
                    .OrderBy(s => s.Date).ToListAsync();
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
                    .Where(s => (DateFrom <= s.Date && s.Date <= DateTo))
                    .OrderBy(s => s.Date).ToListAsync();
            }
            
            return View(new ScheduleListViewModel
            {
                isAdmin = isAdmin,
                isDoctor = isDoctor,
                isPatient = isPatient,
                Schedules = schedules,
                DateFrom = DateFrom,
                DateTo = DateTo
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

            newSchedules.ForEach(el => el.Date = el.Date.AddDays(7));

            var combinedSchedules = oldSchedules.Concat(newSchedules).OrderBy(el => el.Date).ToList();


            return View(new ScheduleCopyListViewModel
            {
                OldSchedules = oldSchedules,
                NewSchedules = newSchedules,
                CombinedSchedules = combinedSchedules,
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

            var oldSchedules = await _context.Schedule.Include(s => s.Doctor)
                .Include(s => s.Doctor.Specialization)
                .Include(s => s.Patient)
                .Where(s => (endOfWeek <= s.Date && s.Date <= endOfNextWeek))
                .OrderBy(s => s.Date).ToListAsync();

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
            });

            await _context.AddRangeAsync(newSchedules);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = startOfWeek, DateTo = endOfWeek});
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
        public async Task<IActionResult> Create([Bind("Date,DoctorId")] ScheduleCreateOrEditViewModel scheduleCreateViewModel)
        {
            if (scheduleCreateViewModel.Date == null)
            {
                return BadRequest();
            }
            Schedule schedule = new Schedule { Date=(DateTime) scheduleCreateViewModel.Date};
            if (!System.String.IsNullOrEmpty(scheduleCreateViewModel.DoctorId))
            {
                var doctor = await _context.Employee.FindAsync(scheduleCreateViewModel.DoctorId);
                if (doctor == null)
                {
                    return NotFound("Doctor not found");
                }
                schedule.Doctor = doctor;
            }
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule.Include(s => s.Doctor).Include(s => s.Patient).FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                return NotFound();
            }
            var model = new ScheduleCreateOrEditViewModel
            {
                Date = schedule.Date,
                Id = schedule.Id,
                Doctors = await _context.Employee.Include(e => e.Specialization).ToListAsync()
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,DoctorId")] ScheduleCreateOrEditViewModel scheduleEditViewModel)
        {
            if (id != scheduleEditViewModel.Id)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule.Include(s => s.Patient).Include(s => s.Doctor).FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
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
            return View(schedule);
        }

        // POST: Schedules/Book/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Book(int id, DateTime dateFrom, DateTime dateTo)
        {
            var schedule = await _context.Schedule.Include(s => s.Patient).FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
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
            return RedirectToAction(nameof(Index), "Schedules", new {DateFrom = dateFrom, DateTo=dateTo});
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
                _context.Schedule.Remove(schedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedule.Any(e => e.Id == id);
        }
    }
}

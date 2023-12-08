using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Data;
using MvcClinic.Models;
using NuGet.Protocol;

namespace MvcClinic.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    public class PatientsController : Controller
    {
        private readonly MvcClinicContext _context;
        private readonly UserManager<Patient> _patientManager;

        public PatientsController(MvcClinicContext context, UserManager<Patient> patientManager)
        {
            _context = context;
            _patientManager = patientManager;
        }

        // GET: Patients
        public async Task<IActionResult> Index(string patientSurname, string searchString)
        {
            if (_context.Patient == null)
            {
                return Problem("Entity set 'MvcClinicContext.Clinic' is null.");
            }

            // Use LINQ to get list of surnames.
            IQueryable<string> surnameQuery = from p in _context.Patient
                                            orderby p.Surname
                                            select p.Surname;

            var patients = from p in _context.Patient
                         select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(s => s.FirstName!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(patientSurname))
            {
                patients = patients.Where(x => x.Surname == patientSurname);
            }

            var patientSurnameVM = new PatientSurnameViewModel
            {
                Surnames = new SelectList(await surnameQuery.Distinct().ToListAsync()),
                Patients = await patients.OrderBy(x => x.Surname).ToListAsync()
            };

            return View(patientSurnameVM);
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Edit/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(new PatientEditViewModel{ Id = patient.Id, FirstName = patient.FirstName, Surname=patient.Surname, Active=patient.Active});
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,Surname,Active")] PatientEditViewModel patientEditModel)
        {
            var newFirstName = patientEditModel.FirstName;
            var newSurname = patientEditModel.Surname;
            var newActive = patientEditModel.Active;

            if (id != patientEditModel.Id || id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient.FindAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            patient.FirstName = newFirstName;
            patient.Surname = newSurname;
            patient.Active = newActive;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            return View(patient);
        }

        // GET: Patients/Delete/5
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var patient = await _context.Patient.FindAsync(id);
            if (patient != null)
            {
                _context.Patient.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(string id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}

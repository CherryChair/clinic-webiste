using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Data;
using MvcClinic.Models;

namespace MvcClinic.Controllers
{
    public class PatientsController : Controller
    {
        private readonly MvcClinicContext _context;

        public PatientsController(MvcClinicContext context)
        {
            _context = context;
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
                Patients = await patients.ToListAsync()
            };

            return View(patientSurnameVM);
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
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

        public string HashPassword(string password)
        {
            return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password)));
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] PatientLoginData patientLoginData)
        {
            if (!ModelState.IsValid)
            {
                return View(patientLoginData);
            }

            var patient = await _context.Patient
                .FirstOrDefaultAsync(m => (m.Email == patientLoginData.Email) && (m.Password == HashPassword(patientLoginData.Password)));
            if (patient == null)
            {
                return View(patientLoginData);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Register()
        {
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,FirstName,DateOfBirth,Surname,Email,Password")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                patient.Password = HashPassword(patient.Password);
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,DateOfBirth,Surname,Credit,Active")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,DateOfBirth,Surname,Credit,Active")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Delete(int? id)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patient.FindAsync(id);
            if (patient != null)
            {
                _context.Patient.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}

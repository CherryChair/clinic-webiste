using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MvcClinic.Areas.Identity.Pages.Account;
using MvcClinic.Data;
using MvcClinic.DTOs;
using MvcClinic.Models;
using NuGet.Protocol;

namespace MvcClinic.Controllers
{
    [Authorize(Policy = "DoctorOnly")]
    public class PatientsController : Controller
    {
        private readonly MvcClinicContext _context;
        private readonly UserManager<Patient> _patientManager;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUserStore<Patient> _userStore;
        private readonly IUserEmailStore<Patient> _emailStore;
        private readonly IConfiguration _configuration;
        public PatientsController(MvcClinicContext context, UserManager<Patient> patientManager, UserManager<UserAccount> userManager, IConfiguration configuration, 
                IUserStore<Patient> userStore)
        {
            _context = context;
            _patientManager = patientManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<Patient>)_userStore;
            _configuration = configuration;
        }

        private Patient CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Patient>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Patient)}'. " +
                    $"Ensure that '{nameof(Patient)}' is not an abstract class and has a parameterless constructor");
            }
        }

        [HttpPost("[controller]/register")]
        [AllowAnonymous]
        public async Task<ActionResult<Patient>> Register([FromBody] RegisterDTO model)
        {
            if (model.Email == null || model.Password == null || model.FirstName == null || model.Surname == null)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                return Conflict();
            }
            var patient = CreateUser();
            patient.FirstName = model.FirstName;
            patient.Surname = model.Surname;

            await _userStore.SetUserNameAsync(patient, model.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(patient, model.Email, CancellationToken.None);

            var result = await _patientManager.CreateAsync(patient, model.Password);

            if (result.Succeeded)
            {
                await _patientManager.AddClaimAsync(patient, new Claim("IsPatient", "true"));
                var tokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                };
                var userClaims = await _patientManager.GetClaimsAsync(patient);
                tokenClaims.AddRange(userClaims);
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: tokenClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return BadRequest();
        }

        // GET: Patients
        [HttpGet("[controller]/index")]
        public async Task<ActionResult<IEnumerable<Patient>>> Index(string patientSurname, string searchString)
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

            //var patientSurnameVM = new PatientSurnameViewModel
            //{
            //    Surnames = new SelectList(await surnameQuery.Distinct().ToListAsync()),
            //    Patients = await patients.OrderBy(x => x.Surname).ToListAsync()
            //};

            return await patients.OrderBy(x => x.Surname).ToListAsync();
        }

        [HttpGet("[controller]/details")]
        // GET: Patients/Details/5
        public async Task<ActionResult<Patient>> Details(string? id)
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

            return patient;
        }

        // GET: Patients/Edit/5
        [HttpGet("[controller]/edit")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PatientEditViewDTO>> Edit(string? id)
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
            return new PatientEditViewDTO{ 
                Id = patient.Id, 
                FirstName = patient.FirstName, 
                Surname=patient.Surname, 
                Active=patient.Active, 
                ConcurrencyStamp = patient.ConcurrencyStamp
            };
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/edit")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Edit(string id, [Bind("Id,FirstName,Surname,Active,ConcurrencyStamp")] PatientEditViewDTO patientEditViewModel)
        {
            var newFirstName = patientEditViewModel.FirstName;
            var newSurname = patientEditViewModel.Surname;
            var newActive = patientEditViewModel.Active;

            if (id != patientEditViewModel.Id || id == null)
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
            _context.Entry(patient).OriginalValues["ConcurrencyStamp"] = patientEditViewModel.ConcurrencyStamp;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    patient.ConcurrencyStamp = Guid.NewGuid().ToString();
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
                        return BadRequest("Concurrency exception");
                        //TempData["ConcurrencyExceptionPatient"] = true;
                        //await _context.Entry(patient).ReloadAsync();
                        //patientEditViewModel.FirstName = patient.FirstName;
                        //patientEditViewModel.Surname = patient.Surname;
                        //patientEditViewModel.Active = patient.Active;
                    }
                }
            }
            //ModelState.Clear();
            //patientEditViewModel.ConcurrencyStamp = patient.ConcurrencyStamp;

            return Ok();
        }

        // GET: Patients/Delete/5
        [HttpGet("[controller]/delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Patient>> Delete(string? id)
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

            return patient;
        }

        // POST: Patients/Delete/5
        [HttpPost("[controller]/delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteConfirmed(string id, string? concurrencyStamp)
        {
            var patient = await _context.Patient.FindAsync(id);

            if (patient != null)
            {
                _context.Entry(patient).OriginalValues["ConcurrencyStamp"] = concurrencyStamp;
                try
                {
                    _context.Patient.Remove(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
                    {
                        //TempData["ConcurrencyExceptionPatientAlreadyDeleted"] = true;
                        return NotFound(patient.Id);
                    }
                    else
                    {
                        return BadRequest("Concurrency exception");
                    }
                }
            }

            return Ok();
        }

        private bool PatientExists(string id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}

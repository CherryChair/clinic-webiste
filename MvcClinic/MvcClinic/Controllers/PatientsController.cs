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
        [HttpGet("[controller]/list")]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> List()
        {
            var patients = from p in _context.Patient
                           select new PatientDTO { Id = p.Id, FirstName = p.FirstName, Surname = p.Surname, Email = p.Email, Active = p.Active, ConcurrencyStamp = p.ConcurrencyStamp };

            return await patients.OrderBy(x => x.Surname).ToListAsync();
        }

        [HttpGet("[controller]")]
        public async Task<ActionResult<PatientDTO>> Details([FromQuery] string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patients = from p in _context.Patient
                           where p.Id == id
                           select new PatientDTO { Id = p.Id, FirstName = p.FirstName, Surname = p.Surname, Email = p.Email, Active = p.Active, ConcurrencyStamp = p.ConcurrencyStamp };

            var patient = await patients.FirstOrDefaultAsync();
            if (patient == null)
            {
                return NotFound();
            }

            return patient;
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/edit")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<string>> Edit([FromBody] PatientDTO patientDTO)
        {

            if (patientDTO.Id == null)
            {
                return NotFound("id");
            }

            var patient = await _context.Patient.FindAsync(patientDTO.Id);

            if (patient == null)
            {
                return NotFound("patient");
            }

            patient.FirstName = patientDTO.FirstName;
            patient.Surname = patientDTO.Surname;
            patient.Email = patientDTO.Email;
            patient.Active = patientDTO.Active;
            _context.Entry(patient).OriginalValues["ConcurrencyStamp"] = patientDTO.ConcurrencyStamp;

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
                        return Conflict("Concurrency exception");
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

            return patient.ConcurrencyStamp;
        }

        // POST: Patients/Delete/5
        [HttpPost("[controller]/delete")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteConfirmed([FromBody] DeleteDTO requestBody)
        {
            var patient = await _context.Patient.FindAsync(requestBody.Id);

            if (patient != null)
            {
                _context.Entry(patient).OriginalValues["ConcurrencyStamp"] = requestBody.ConcurrencyStamp;
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
                        return StatusCode(410);
                    }
                    else
                    {
                        return Conflict("Concurrency exception");
                    }
                }
            } else { 
                return StatusCode(410); 
            }

            return Ok();
        }

        private bool PatientExists(string id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}

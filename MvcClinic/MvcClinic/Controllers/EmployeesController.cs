using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MvcClinic.Data;
using MvcClinic.Models;

namespace MvcClinic.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController : Controller
    {
        private readonly MvcClinicContext _context;
        private readonly UserManager<Employee> _employeeManager;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUserStore<Employee> _userStore;
        private readonly IUserEmailStore<Employee> _emailStore;
        private readonly IConfiguration _configuration;

        public EmployeesController(MvcClinicContext context, UserManager<Employee> employeeManager, UserManager<UserAccount> userManager, IConfiguration configuration,
                IUserStore<Employee> userStore)
        {
            _context = context;
            _employeeManager = employeeManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<Employee>)_userStore;
            _configuration = configuration;
        }

        private Employee CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Employee>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Employee)}'. " +
                    $"Ensure that '{nameof(Employee)}' is not an abstract class and has a parameterless constructor");
            }
        }

        [HttpPost("[controller]/register")]
        public async Task<ActionResult<Employee>> Register([FromBody] RegisterEmployeeModel model)
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
            var employee = CreateUser();
            employee.FirstName = model.FirstName;
            employee.Surname = model.Surname;
            var spec = await _context.Speciality.FirstOrDefaultAsync(m => m.Id == model.SpecializationId);
            employee.Specialization = spec;

            await _userStore.SetUserNameAsync(employee, model.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(employee, model.Email, CancellationToken.None);

            var result = await _employeeManager.CreateAsync(employee, model.Password);

            if (result.Succeeded)
            {
                await _employeeManager.AddClaimAsync(employee, new Claim("IsEmployee", "true"));
                var tokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                };
                var userClaims = await _employeeManager.GetClaimsAsync(employee);
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

        // GET: Employees
        [HttpGet("[controller]/index")]
        public async Task<ActionResult<IEnumerable<Employee>>> Index()
        {
            return Ok(await _context.Employee.Include(e => e.Specialization).OrderBy(e => e.Surname).ToListAsync());
            //return await _context.Employee.Include(e => e.Specialization).OrderBy(e => e.Surname).ToListAsync();
        }

        // GET: Employees/Details/5
        [HttpGet("[controller]/details")]
        public async Task<ActionResult<Employee>> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.Include(e => e.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        [HttpGet("[controller]/edit")]
        // GET: Employees/Edit/5
        public async Task<ActionResult<EmployeeEditViewModel>> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            IQueryable<string> specialityQuery = from p in _context.Speciality
                                                 select p.Name;
            var specialities = new SelectList(await specialityQuery.Distinct().ToListAsync());

            var employee = await _context.Employee.Include(e => e.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            string? spec = "";
            if (employee.Specialization != null)
            {
                spec = employee.Specialization.Name;
            }
            return new EmployeeEditViewModel { 
                Id=employee.Id,
                FirstName=employee.FirstName,
                Surname=employee.Surname,
                Speciality=spec,
                Specialities=specialities,
                ConcurrencyStamp=employee.ConcurrencyStamp,
            };
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/edit")]
        public async Task<ActionResult> Edit(string id, [Bind("Id,FirstName,Surname,Speciality,ConcurrencyStamp")] EmployeeEditViewModel employeeEditViewModel)
        {
            if (id != employeeEditViewModel.Id)
            {
                return NotFound();
            }

            var employee = await _context.Employee.Include(m => m.Specialization).FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            employee.FirstName = employeeEditViewModel.FirstName;
            employee.Surname = employeeEditViewModel.Surname;

            var spec = await _context.Speciality.FirstOrDefaultAsync(m => m.Name == employeeEditViewModel.Speciality);
            employee.Specialization = spec;

            _context.Entry(employee).OriginalValues["ConcurrencyStamp"] = employeeEditViewModel.ConcurrencyStamp;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    employee.ConcurrencyStamp = Guid.NewGuid().ToString();
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return BadRequest("Concurrency exception");
                        //TempData["ConcurrencyExceptionEmployee"] = true;
                        //await _context.Entry(employee).ReloadAsync();
                        //employeeEditViewModel.FirstName = employee.FirstName;
                        //employeeEditViewModel.Surname = employee.Surname;
                        //if (employee.Specialization != null)
                        //{
                        //    employeeEditViewModel.Speciality = employee.Specialization.Name;
                        //} else
                        //{
                        //    employeeEditViewModel.Speciality = "";
                        //}
                    }
                }
            }
            //ModelState.Clear();
            //employeeEditViewModel.ConcurrencyStamp = employee.ConcurrencyStamp;

            //IQueryable<string> specialityQuery = from p in _context.Speciality
            //                                     select p.Name;
            //var specialities = new SelectList(await specialityQuery.Distinct().ToListAsync());
            //employeeEditViewModel.Specialities= specialities;

            return NoContent();
        }

        // GET: Employees/Delete/5
        [HttpGet("[controller]/delete")]
        public async Task<ActionResult<Employee>> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.Include(e => e.Specialization)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // POST: Employees/Delete/5
        [HttpPost("[controller]/delete")]
        public async Task<ActionResult> DeleteConfirmed(string id, string? concurrencyStamp)
        {
            var employee = await _context.Employee.FindAsync(id);

            if (employee != null)
            {
                _context.Entry(employee).OriginalValues["ConcurrencyStamp"] = concurrencyStamp;
                try
                {
                    _context.Employee.Remove(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        //TempData["ConcurrencyExceptionEmployeeAlreadyDeleted"] = true;
                        return BadRequest("Employee already delted");
                    }
                    else
                    {
                        //TempData["ConcurrencyExceptionEmployeeDelete"] = true;
                        return BadRequest("Concurrency exception");
                    }
                }
            }

            return NoContent();
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}

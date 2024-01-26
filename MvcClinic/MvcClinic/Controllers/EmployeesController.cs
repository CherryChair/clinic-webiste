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
using MvcClinic.DTOs;
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
        public async Task<ActionResult> Register([FromBody] RegisterEmployeeDTO model)
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
                await _employeeManager.AddClaimAsync(employee, new Claim("IsDoctor", "true"));
                return Ok();
            }
            return BadRequest();
        }

        // GET: Employees
        [HttpGet("[controller]/list")]
        public async Task<ActionResult<IEnumerable<EmployeeDTO>>> List()
        {
            var employees = from e in _context.Employee
                           select new EmployeeDTO { Id = e.Id, FirstName = e.FirstName, Surname = e.Surname, Email = e.Email, SpecialityId = e.Specialization.Id, ConcurrencyStamp = e.ConcurrencyStamp };
            return await employees.OrderBy(x => x.Surname).ToListAsync();
            //return await _context.Employee.Include(e => e.Specialization).OrderBy(e => e.Surname).ToListAsync();
        }

        [HttpGet("[controller]/idList")]
        public async Task<ActionResult<IEnumerable<EmployeeIdDTO>>> IdList()
        {
            var employees = from e in _context.Employee
                            select new EmployeeIdDTO { Id = e.Id, Name = e.Specialization == null ? 
                                e.FirstName + " " + e.Surname :
                                e.FirstName + " " + e.Surname + " [" + e.Specialization.Name + "]"};
            return await employees.ToListAsync();
        }

        // GET: Employees/Details/5
        [HttpGet("[controller]")]
        public async Task<ActionResult<EmployeeDTO>> Details([FromQuery] string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employees = from e in _context.Employee
                            where e.Id == id
                            select new EmployeeDTO { Id = e.Id, FirstName = e.FirstName, Surname = e.Surname, Email = e.Email, SpecialityId = e.Specialization.Id, ConcurrencyStamp = e.ConcurrencyStamp };

            var employee = await employees.FirstOrDefaultAsync();
            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("[controller]/edit")]
        public async Task<ActionResult<string>> Edit([FromBody] EmployeeDTO employeeDTO)
        {
            if (employeeDTO.Id == null)
            {
                return NotFound();
            }
            //IQueryable<string> specialityQuery = from p in _context.Speciality
            //                                     select p.Name;
            //var specialities = new SelectList(await specialityQuery.Distinct().ToListAsync());

            var employee = await _context.Employee.Include(e => e.Specialization)
                .FirstOrDefaultAsync(m => m.Id == employeeDTO.Id);

            if (employee == null)
            {
                return NotFound();
            }

            employee.FirstName = employeeDTO.FirstName;
            employee.Surname = employeeDTO.Surname;
            if (employee.Email != employeeDTO.Email)
            {
                if (await _userManager.FindByEmailAsync(employeeDTO.Email) != null)
                {
                    return BadRequest();
                }
            }
            employee.Email = employeeDTO.Email;
            var speciality = await _context.Speciality.FirstOrDefaultAsync(s => s.Id == employeeDTO.SpecialityId);
            employee.Specialization = speciality;

            _context.Entry(employee).OriginalValues["ConcurrencyStamp"] = employeeDTO.ConcurrencyStamp;

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
                        return Conflict("Concurrency exception");
                    }
                }
            }

            return employee.ConcurrencyStamp;
        }

        // POST: Employees/Delete/5
        [HttpPost("[controller]/delete")]
        public async Task<ActionResult> DeleteConfirmed([FromBody] DeleteDTO requestBody)
        {
            var employee = await _context.Employee.FindAsync(requestBody.Id);

            if (employee != null)
            {
                _context.Entry(employee).OriginalValues["ConcurrencyStamp"] = requestBody.ConcurrencyStamp;
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

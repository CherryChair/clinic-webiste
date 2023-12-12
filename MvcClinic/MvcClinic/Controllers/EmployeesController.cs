using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Data;
using MvcClinic.Models;

namespace MvcClinic.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController : Controller
    {
        private readonly MvcClinicContext _context;

        public EmployeesController(MvcClinicContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employee.Include(e => e.Specialization).OrderBy(e => e.Surname).ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(string? id)
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

            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(string? id)
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
            return View(new EmployeeEditViewModel { 
                Id=employee.Id,
                FirstName=employee.FirstName,
                Surname=employee.Surname,
                Speciality=spec,
                Specialities=specialities,
                ConcurrencyStamp=employee.ConcurrencyStamp,
            });
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,Surname,Speciality,ConcurrencyStamp")] EmployeeEditViewModel employeeEditViewModel)
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
                        TempData["ConcurrencyExceptionEmployee"] = true;
                        await _context.Entry(employee).ReloadAsync();
                        employeeEditViewModel.FirstName = employee.FirstName;
                        employeeEditViewModel.Surname = employee.Surname;
                        if (employee.Specialization != null)
                        {
                            employeeEditViewModel.Speciality = employee.Specialization.Name;
                        } else
                        {
                            employeeEditViewModel.Speciality = "";
                        }
                    }
                }
            }
            ModelState.Clear();
            employeeEditViewModel.ConcurrencyStamp = employee.ConcurrencyStamp;

            IQueryable<string> specialityQuery = from p in _context.Speciality
                                                 select p.Name;
            var specialities = new SelectList(await specialityQuery.Distinct().ToListAsync());
            employeeEditViewModel.Specialities= specialities;

            return View(employeeEditViewModel);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(string? id)
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

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var employee = await _context.Employee.FindAsync(id);

            if (employee != null)
            {
                try
                {
                    _context.Employee.Remove(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        TempData["ConcurrencyExceptionEmployeeAlreadyDeleted"] = true;
                    }
                    else
                    {
                        TempData["ConcurrencyExceptionEmployeeDelete"] = true;
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}

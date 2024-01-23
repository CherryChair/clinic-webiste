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
    [Authorize(Policy = "DoctorOnly")]
    public class SpecialitiesController : Controller
    {
        private readonly MvcClinicContext _context;

        public SpecialitiesController(MvcClinicContext context)
        {
            _context = context;
        }

        // GET: Specialities
        [HttpGet("[controller]/list")]
        public async Task<ActionResult<ICollection<Speciality>>> Index()
        {
            return await _context.Speciality.ToListAsync();
        }

    }
}

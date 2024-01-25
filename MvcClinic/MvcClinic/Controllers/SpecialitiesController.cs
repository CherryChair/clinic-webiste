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
using MvcClinic.DTOs;

namespace MvcClinic.Controllers
{
    public class SpecialitiesController : Controller
    {
        private readonly MvcClinicContext _context;

        public SpecialitiesController(MvcClinicContext context)
        {
            _context = context;
        }

        // GET: Specialities
        [HttpGet("[controller]/list")]
        [AllowAnonymous]
        public async Task<IEnumerable<SepcializationDTO>> Index()
        {
            var specialities = from s in _context.Speciality
                               select new SepcializationDTO { Id = s.Id, Name = s.Name };
            return await specialities.ToListAsync();
        }
    }
}

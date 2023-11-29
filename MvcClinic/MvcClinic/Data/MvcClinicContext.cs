using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MvcClinic.Models;

namespace MvcClinic.Data
{
    public class MvcClinicContext : DbContext
    {
        public MvcClinicContext (DbContextOptions<MvcClinicContext> options)
            : base(options)
        {
        }

        public DbSet<MvcClinic.Models.Patient> Patient { get; set; } = default!;
    }
}

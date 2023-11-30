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
        public DbSet<MvcClinic.Models.Employee> Employee { get; set; } = default!;
        public DbSet<MvcClinic.Models.Speciality> Speciality { get; set; } = default!;
        public DbSet<MvcClinic.Models.Schedule> Schedule { get; set; } = default!;
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity;

public enum EmployeeType
{
    Director,
    Doctor
}


namespace MvcClinic.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Employee : UserAccount
    {
        public EmployeeType Type { get; set; }
        public Speciality? Specialization { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

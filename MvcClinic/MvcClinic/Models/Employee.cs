using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

public enum EmployeeType
{
    Director,
    Doctor
}


namespace MvcClinic.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Employee
    {
        public int Id { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string Surname { get; set; }
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public EmployeeType Type { get; set; }
        public Speciality? Specialization { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

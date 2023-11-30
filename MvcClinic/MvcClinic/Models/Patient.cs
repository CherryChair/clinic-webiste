using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Patient
    {
        public int Id { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string FirstName { get; set; }
        [Required]
        [StringLength(30)]
        public string Surname { get; set; }
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        [Required]
        public bool Active { get; set; } = false;
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Password { get; set; }
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

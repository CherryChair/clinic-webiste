using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? FirstName { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? Surname { get; set; }
        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}

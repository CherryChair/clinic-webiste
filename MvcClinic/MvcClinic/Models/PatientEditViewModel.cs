using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.Models
{
    public class PatientEditViewModel
    {
        public string? Id { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [DisplayName("First Name")]
        [Required]
        public string? FirstName { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? Surname { get; set; }
        [Required]
        public bool Active { get; set; } = false;
        public string? ConcurrencyStamp { get; set; }
    }
}

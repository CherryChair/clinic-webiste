using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class PatientLoginData

    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MvcClinic.DTOs
{
    public class LoginDTO
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}

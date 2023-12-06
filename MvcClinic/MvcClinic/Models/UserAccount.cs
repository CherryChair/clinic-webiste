using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class UserAccount : IdentityUser
    {
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? FirstName { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? Surname { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.Models
{
    public class EmployeeEditViewModel
    {
        public string? Id { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [DisplayName("First Name")]
        [Required]
        public string? FirstName { get; set; }
        [StringLength(60, MinimumLength = 1)]
        [Required]
        public string? Surname { get; set; }
        public SelectList? Specialities { get; set; }
        public string? Speciality { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }
}

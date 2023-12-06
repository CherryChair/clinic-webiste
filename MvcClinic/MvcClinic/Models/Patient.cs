using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Patient : Account
    {
        [Required]
        public bool Active { get; set; } = false;
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

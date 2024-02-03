using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime Date { get; set; }
        public Patient? Patient { get; set; }
        public Employee? Doctor { get; set; }
        [StringLength(2000)]
        public string? Description { get; set; }

        [ConcurrencyCheck]
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}

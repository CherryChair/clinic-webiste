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
    }
}

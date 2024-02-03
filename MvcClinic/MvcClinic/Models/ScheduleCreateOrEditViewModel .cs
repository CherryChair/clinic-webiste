using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.Models
{
    public class ScheduleCreateOrEditViewModel
    {
        public int? Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime? Date { get; set; }
        public string? PatientId { get; set; }
        public string? Patient { get; set; }
        [DisplayName("Doctor")]
        public string? DoctorId { get; set; }
        public List<Employee>? Doctors { get; set;}
        [StringLength(2000)]
        public string? Description { get; set; }
        public bool? IsDoctor { get; set; }
        public Guid ConcurrencyStamp { get; set; }
}
}

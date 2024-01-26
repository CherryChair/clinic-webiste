using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.DTOs
{
    public class ScheduleListDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.DTOs
{
    public class ScheduleDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public PatientDTO? Patient { get; set; }
        public EmployeeDTO? Doctor { get; set; }
        public string? Description { get; set; }
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}

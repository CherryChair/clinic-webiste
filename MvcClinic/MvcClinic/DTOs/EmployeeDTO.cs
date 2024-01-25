using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.DTOs
{
    public class EmployeeDTO
    {
        public string? Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int SpecialityId { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace MvcClinic.DTOs
{
    public class EmployeeIdDTO
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}

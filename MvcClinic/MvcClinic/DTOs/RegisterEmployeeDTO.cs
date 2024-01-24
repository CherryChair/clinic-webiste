using System.ComponentModel.DataAnnotations;

namespace MvcClinic.DTOs
{
    public class RegisterEmployeeDTO : RegisterDTO
    {

        public int? SpecializationId { get; set; }
    }
}


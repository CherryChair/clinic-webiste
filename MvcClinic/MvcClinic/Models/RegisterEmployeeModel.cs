using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class RegisterEmployeeModel : RegisterModel
    {

        public int ? SpecializationId { get; set; }
    }
}


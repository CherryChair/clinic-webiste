using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MvcClinic.Models
{
    public class PatientSurnameViewModel
    {
        public List<Patient>? Patients { get; set; }
        public SelectList? Surnames { get; set; }
        public string? PatientSurname { get; set; }
        public string? SearchString { get; set; }
    }
}

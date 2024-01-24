using Microsoft.AspNetCore.Mvc.Rendering;
using MvcClinic.Models;
using System.Collections.Generic;

namespace MvcClinic.DTOs
{
    public class PatientSurnameViewDTO
    {
        public List<Patient>? Patients { get; set; }
        public SelectList? Surnames { get; set; }
        public string? PatientSurname { get; set; }
        public string? SearchString { get; set; }
    }
}

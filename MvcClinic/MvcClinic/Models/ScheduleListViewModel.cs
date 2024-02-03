using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class ScheduleListViewModel
    {
        public bool isAdmin {  get; set; }=false;
        public bool isDoctor {  get; set; }=false;
        public bool isPatient {  get; set; }=false;
        public List<Schedule> Schedules { get; set;}
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        [DisplayName("Date To")]
        [Required]
        public DateTime DateTo { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        [DisplayName("Date From")]
        [Required]
        public DateTime DateFrom { get; set; }
        public List<Speciality> Specalities { get; set; }
        [DisplayName("Speciality")]
        public int? SpecialityId { get; set; }
    }
}

using System.ComponentModel;

namespace MvcClinic.DTOs
{
    public class ReportEntry
    {
        [DisplayName("Name")]
        public string DoctorName { get; set; }
        [DisplayName("Specialiaztion")]
        public string? DoctorSpecialization { get; set; }
        [DisplayName("Time Worked")]
        public int? PastSchedulesNumber { get; set; }
        [DisplayName("Visits Number")]
        public int? PastSchedulesWithPatientNumber { get; set; }
        [DisplayName("Scheduled Time")]
        public int? FutureSchedulesNumber { get; set; }
        [DisplayName("Future Scheduled Visits")]
        public int? FutureSchedulesWithPatientNumber { get; set; }
    }
}

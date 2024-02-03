using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class ScheduleReportViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.Date)]
        [DisplayName("Date From")]
        [Required]
        public DateOnly DateFrom { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.Date)]
        [DisplayName("Date To")]
        [Required]
        public DateOnly DateTo { get; set; }
        public List<ReportEntry> ReportEntries { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class ScheduleCopyListViewModel
    {
        public List<Schedule>? OldSchedules { get; set; }
        public List<Schedule>? NewSchedules { get; set; }
        public List<Schedule>? CombinedSchedules { get; set; }
        public List<Schedule>? ConflictingSchedules { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.DateTime)]
        [DisplayName("Date To")]
        [Required]
        public DateTime? DateTo { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.DateTime)]
        [DisplayName("Date From")]
        [Required]
        public DateTime? DateFrom { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MvcClinic.Models;

namespace MvcClinic.DTOs
{
    public class ScheduleCopyListViewDTO
    {
        public List<ScheduleDTO>? OldSchedules { get; set; }
        public List<ScheduleDTO>? NewSchedules { get; set; }
        public List<ScheduleDTO>? CombinedSchedules { get; set; }
        public List<ScheduleDTO>? ConflictingSchedules { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DateFrom { get; set; }
    }
}

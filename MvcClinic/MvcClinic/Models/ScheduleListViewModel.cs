namespace MvcClinic.Models
{
    public class ScheduleListViewModel
    {
        public bool isAdmin {  get; set; }=false;
        public bool isDoctor {  get; set; }=false;
        public bool isPatient {  get; set; }=false;
        public List<Schedule>? Schedules { get; set;}
    }
}

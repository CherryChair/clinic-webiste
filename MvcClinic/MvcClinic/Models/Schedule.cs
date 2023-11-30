namespace MvcClinic.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Patient? Patient { get; set; }
        public Employee? Doctor { get; set; }
    }
}

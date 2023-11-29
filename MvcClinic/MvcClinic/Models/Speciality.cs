namespace MvcClinic.Models
{
    public class Speciality
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<Employee> Doctors { get; set; } = new List<Employee>();
    }
}

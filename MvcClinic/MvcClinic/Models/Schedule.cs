using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MvcClinic.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}")]
        [DataType(DataType.DateTime)]
        [Required]
        public DateTime Date { get; set; }
        public Patient? Patient { get; set; }
        public Employee? Doctor { get; set; }
        [StringLength(2000)]
        public string? Description { get; set; }
        //public DateTime ConcurrencyToken { get; set; } = DateTime.Now;
    }

    //internal class ScheduleEntityTypeConfigurationSqlite : IEntityTypeConfiguration<Schedule>
    //{
    //    public void Configure(EntityTypeBuilder<Schedule> builder)
    //    {
    //        builder.ToTable("Schedule");
    //        builder.HasKey(x => x.Id);
    //        builder.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
    //        builder.Property(x => x.ConcurrencyToken).HasColumnName("CocurrencyToken").HasConversion<DateTime>()
    //            .IsConcurrencyToken();
    //    }
    //}

}

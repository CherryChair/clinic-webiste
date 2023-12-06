using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcClinic.Data;
using System;
using System.Linq;

namespace MvcClinic.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new MvcClinicContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<MvcClinicContext>>()))
        {
            return;
//            // Look for any records in database.
//            if (context.Patient.Any() || context.Employee.Any() || context.Speciality.Any() || context.Schedule.Any())
//            {
//                return;   // DB has been seeded
//            }
//            foreach (var patient in context.Patient)
//            {
//                context.Patient.Remove(patient);
//            }
//            foreach (var employee in context.Employee)
//            {
//                context.Employee.Remove(employee);
//            }
//            foreach (var speciality in context.Speciality)
//            {
//                context.Speciality.Remove(speciality);
//            }
//            foreach (var schedule in context.Schedule)
//            {
//                context.Schedule.Remove(schedule);
//            }
//            //context.SaveChanges();
//            Patient[] patientsArray =
//            {
//                new Patient
//                {
//                    FirstName = "Michał",
//                    DateOfBirth = DateTime.Parse("1989-2-12"),
//                    Surname = "Kowalczyk",
//                    Active = true,
//                    Email = "mpkowalcz@wp.pl",
//                    Password = "root",
//                },
//                new Patient
//                {
//                    FirstName = "Alicja",
//                    DateOfBirth = DateTime.Parse("1959-4-15"),
//                    Surname = "Alicjańska",
//                    Active = false,
//                    Email = "mpkowalcz2@wp.pl",
//                    Password = "root",
//                }
//            };
//            Speciality[] specialitiesArray =
//{
//                new Speciality
//                {
//                    Name = "Karidolog"
//                },
//                new Speciality
//                {
//                    Name = "Podolog"
//                },
//                new Speciality
//                {
//                    Name = "Anestezjolog"
//                }
//            };
//            Employee[] employeesArray =
//            {
//                new Employee
//                {
//                    FirstName = "Jan",
//                    DateOfBirth = DateTime.Parse("1980-4-15"),
//                    Surname = "Janowski",
//                    Email = "mpkowalcz01@wp.pl",
//                    Password = "root",
//                    Type = EmployeeType.Director
//                },
//                new Employee
//                {
//                    FirstName = "Adam",
//                    DateOfBirth = DateTime.Parse("1930-3-15"),
//                    Surname = "Adamowski",
//                    Email = "mpkowalcz02@wp.pl",
//                    Password = "root",
//                    Type = EmployeeType.Doctor,
//                    Specialization = specialitiesArray[0],
//                },
//                new Employee
//                {
//                    FirstName = "Maria",
//                    DateOfBirth = DateTime.Parse("1987-4-01"),
//                    Surname = "Mariowska",
//                    Email = "mpkowalcz03@wp.pl",
//                    Password = "root",
//                    Type = EmployeeType.Doctor,
//                    Specialization = specialitiesArray[1],
//                },
//                new Employee
//                {
//                    FirstName = "Janina",
//                    DateOfBirth = DateTime.Parse("1971-9-23"),
//                    Surname = "Janowska",
//                    Email = "mpkowalcz04@wp.pl",
//                    Password = "root",
//                    Type = EmployeeType.Doctor,
//                    Specialization = specialitiesArray[1],
//                },
//                new Employee
//                {
//                    FirstName = "Hubert",
//                    DateOfBirth = DateTime.Parse("1944-6-18"),
//                    Surname = "Hubertowski",
//                    Email = "mpkowalcz05@wp.pl",
//                    Password = "root",
//                    Type = EmployeeType.Doctor,
//                    Specialization = specialitiesArray[2],
//                },
//            };
//            Schedule[] schedulesArray = {
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 12, 0, 0),
//                    Doctor = employeesArray[1]
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 12, 15, 0),
//                    Doctor = employeesArray[1]
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 12, 30, 0),
//                    Doctor = employeesArray[1]
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 12, 45, 0),
//                    Doctor = employeesArray[1]
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 14, 0, 0),
//                    Doctor = employeesArray[4],
//                    Patient = patientsArray[1]
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 7, 14, 15, 0),
//                    Doctor = employeesArray[4],
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 8, 13, 15, 0),
//                    Doctor = employeesArray[3],
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 8, 13, 30, 0),
//                    Doctor = employeesArray[3],
//                },
//                new Schedule
//                {
//                    Date = new DateTime(2023, 12, 8, 13, 45, 0),
//                    Doctor = employeesArray[3],
//                },
//            };
//            //List<Patient> patients = new List<Patient>(patientsArray);
//            context.Patient.AddRange(
//                patientsArray
//            );
//            context.SaveChanges();
        }
    }
}

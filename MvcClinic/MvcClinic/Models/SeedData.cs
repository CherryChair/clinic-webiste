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
            // Look for any movies.
            if (context.Patient.Any())
            {
                return;   // DB has been seeded
            }
            context.Patient.AddRange(
                new Patient
                {
                    FirstName = "Michał",
                    DateOfBirth = DateTime.Parse("1989-2-12"),
                    Surname = "Kowalczyk",
                    Credit = 7.99M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Anna",
                    DateOfBirth = DateTime.Parse("1999-2-12"),
                    Surname = "Kowalczyk",
                    Credit = 12.92M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Joanna",
                    DateOfBirth = DateTime.Parse("2009-3-3"),
                    Surname = "Kowalczyk",
                    Credit = 7.99M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Adam",
                    DateOfBirth = DateTime.Parse("1984-3-13"),
                    Surname = "Adamski",
                    Credit = 8.99M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Jan",
                    DateOfBirth = DateTime.Parse("1986-2-23"),
                    Surname = "Janowski",
                    Credit = 9.99M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Janina",
                    DateOfBirth = DateTime.Parse("1956-12-3"),
                    Surname = "Janowski",
                    Credit = 123.99M,
                    Active = true
                },
                new Patient
                {
                    FirstName = "Alicja",
                    DateOfBirth = DateTime.Parse("1959-4-15"),
                    Surname = "Alicjańska",
                    Credit = 3.99M,
                    Active = false
                }
            );
            context.SaveChanges();
        }
    }
}

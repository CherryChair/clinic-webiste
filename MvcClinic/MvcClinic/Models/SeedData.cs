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
                    Active = true
                },
                new Patient
                {
                    FirstName = "Anna",
                    DateOfBirth = DateTime.Parse("1999-2-12"),
                    Surname = "Kowalczyk",
                    Active = true
                },
                new Patient
                {
                    FirstName = "Joanna",
                    DateOfBirth = DateTime.Parse("2009-3-3"),
                    Surname = "Kowalczyk",
                    Active = true
                },
                new Patient
                {
                    FirstName = "Adam",
                    DateOfBirth = DateTime.Parse("1984-3-13"),
                    Surname = "Adamski",
                    Active = true
                },
                new Patient
                {
                    FirstName = "Jan",
                    DateOfBirth = DateTime.Parse("1986-2-23"),
                    Surname = "Janowski",
                    Active = true
                },
                new Patient
                {
                    FirstName = "Janina",
                    DateOfBirth = DateTime.Parse("1956-12-3"),
                    Surname = "Janowski",
                    Active = true
                },
                new Patient
                {
                    FirstName = "Alicja",
                    DateOfBirth = DateTime.Parse("1959-4-15"),
                    Surname = "Alicjańska",
                    Active = false
                }
            );
            context.SaveChanges();
        }
    }
}

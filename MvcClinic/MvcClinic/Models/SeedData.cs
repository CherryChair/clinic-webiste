using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcClinic.Data;
using System;
using System.Linq;
using System.Security.Claims;

namespace MvcClinic.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new MvcClinicContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<MvcClinicContext>>()))
        {
            if (context.Employee.Any())
            {
                return;
            }
            var AdminUser = new Employee
            {
                Id = "a324cc17-0bee-4702-9f79-365d15717075",
                FirstName = "Admin",
                Surname = "Adminek",
                UserName = "mpkowalcz@wp.pl",
                NormalizedUserName = "MPKOWALCZ@WP.PL",
                Email = "mpkowalcz@wp.pl",
                NormalizedEmail = "MPKOWALCZ@WP.PL",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAIAAYagAAAAEAbjkEruHN3OLDzPPzAvMCWTxuqqcnPLsKaX9hdVmQah8YQK1Q29WLJwo4uBZDXxSg==",
                SecurityStamp = "GWYW5VJBT6WI6OVSXDMC7JL4GUE5AL6R",
                ConcurrencyStamp = "87c78f37-9793-4776-a9cf-1f215ec8ec1a",
                TwoFactorEnabled = false,
                PhoneNumberConfirmed = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            };
            context.Employee.Add(AdminUser);
            Speciality[] specialities = { 
                new Speciality { Name = "Surgeon" },
                new Speciality { Name = "Cardiologist" },
                new Speciality { Name = "Urologist" },
                new Speciality { Name = "Podologist" },
                new Speciality { Name = "Neurologist" },
                new Speciality { Name = "Chiropractor" },
                new Speciality { Name = "Family" }, 
                new Speciality { Name = "Chiropractor" }, 
            };
            context.Speciality.AddRange(specialities);
            context.SaveChanges();

            var _userManager = serviceProvider.GetRequiredService<UserManager<Employee>>();

            _userManager.AddClaimAsync(AdminUser, new Claim("IsAdmin", "true")).Wait();

        }
        //using (var _userManager = serviceProvider.GetRequiredService<UserManager<Employee>>())
    }
}

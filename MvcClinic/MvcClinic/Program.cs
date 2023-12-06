using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MvcClinic.Data;
using MvcClinic.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MvcClinicContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcClinicContext") ?? throw new InvalidOperationException("Connection string 'MvcClinicContext' not found.")));

builder.Services.AddDefaultIdentity<UserAccount>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<MvcClinicContext>();

builder.Services.AddIdentityCore<Employee>().AddEntityFrameworkStores<MvcClinicContext>();
builder.Services.AddIdentityCore<Patient>().AddEntityFrameworkStores<MvcClinicContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MvcClinic.Data;
using MvcClinic.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MvcClinicContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcClinicContext") ?? throw new InvalidOperationException("Connection string 'MvcClinicContext' not found.")));


//builder.Services.AddDefaultIdentity<UserAccount>(options => {
builder.Services.AddIdentityCore<UserAccount>(options => {
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 4;
}).AddEntityFrameworkStores<MvcClinicContext>();

builder.Services.AddIdentityCore<Employee>(options => {
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 4;
}).AddEntityFrameworkStores<MvcClinicContext>().AddSignInManager().AddDefaultTokenProviders();

builder.Services.AddIdentityCore<Patient>(options => {
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 4;
}).AddEntityFrameworkStores<MvcClinicContext>().AddSignInManager().AddDefaultTokenProviders();
//new
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddJwtBearer();
//end new
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorOnly", policy => policy.RequireAssertion(context => (
         context.User.HasClaim(claim => (claim.Type == "IsAdmin" || claim.Type == "IsDoctor")
         )
    )));
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("IsAdmin"));
    options.AddPolicy("PatientOnly", policy => policy.RequireClaim("IsPatient"));
});



// Add services to the container.
//builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

//new
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
//end new

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

app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

//app.MapRazorPages();

app.Run();

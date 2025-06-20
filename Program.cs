using Data.Context;
using MySql.EntityFrameworkCore.Extensions;
using Application;
using Microsoft.AspNetCore.Identity;
using ZstdSharp.Unsafe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


builder.Services.AddMySQLServer<EmployeeAppDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 7;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = true;
    }
)
.AddEntityFrameworkStores<EmployeeAppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2); // Lockout duration
    options.Lockout.MaxFailedAccessAttempts = 3; // Lock after 3 invalid attempts
    options.Lockout.AllowedForNewUsers = true;

    // Optional: Password settings
    options.Password.RequiredLength = 7;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
});



builder.Services.AddServices();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

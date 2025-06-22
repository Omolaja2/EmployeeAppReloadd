using Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using Application;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure MySQL DB context
builder.Services.AddMySQLServer<EmployeeAppDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection")!);

// Configure Identity with Email + 2FA support
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 7;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = true;

    // Sign-in rules
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedPhoneNumber = false; // only email-based 2FA

    // Token providers
    options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<IdentityUser>)));
    options.Tokens.EmailConfirmationTokenProvider = "Email";
    options.Tokens.ChangeEmailTokenProvider = "Email";
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<EmployeeAppDbContext>()
.AddDefaultTokenProviders();

// Configure Identity lockout settings
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});

// Register custom application services
builder.Services.AddServices();

// Register email sender (SMTP like Mailtrap)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

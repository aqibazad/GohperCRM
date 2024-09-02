using LegalAndGeneralConsultantCRM.Areas.Admin;
using LegalAndGeneralConsultantCRM.Areas.Admin.SendNotificationHub;
using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using LegalAndGeneralConsultantCRM.Areas.Admin.AdminDataSeeder;

var builder = WebApplication.CreateBuilder(args);

// Configure the connection string for the database
var connectionString = builder.Configuration.GetConnectionString("LegalAndGeneralConsultantCRMContextConnection")
    ?? throw new InvalidOperationException("Connection string 'LegalAndGeneralConsultantCRMContextConnection' not found.");

// Add DbContext with SQL Server
builder.Services.AddDbContext<LegalAndGeneralConsultantCRMContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity with customized password validation settings
builder.Services.AddIdentity<LegalAndGeneralConsultantCRMUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;           // Remove digit requirement
    options.Password.RequireLowercase = false;       // Remove lowercase requirement
    options.Password.RequireUppercase = false;       // Remove uppercase requirement
    options.Password.RequireNonAlphanumeric = false; // Remove special character requirement
    options.Password.RequiredLength = 6;             // Set minimum password length
})
.AddDefaultTokenProviders()
.AddEntityFrameworkStores<LegalAndGeneralConsultantCRMContext>();

// Configure application cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
});

// Add services for controllers with views, Razor Pages, SignalR, and Email sender
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews().AddRazorPagesOptions(options => {
    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "");
});

var app = builder.Build();

// Seed initial data (e.g., Admin users) if needed
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await AdminSeeder.SeedAdminUserAndRole(serviceProvider);
}

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Identity/Account/Login");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // Map route for areas
    endpoints.MapControllerRoute(
        name: "MyArea",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    // Map default route
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Map SignalR hub
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.MapRazorPages();
app.Run();

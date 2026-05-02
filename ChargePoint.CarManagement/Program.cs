using ChargePoint.CarManagement.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ChargePoint.CarManagement.Application;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using ZiggyCreatures.Caching.Fusion;
using ChargePoint.CarManagement.Middleware;
using ChargePoint.CarManagement.Services.Logging;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs");
Directory.CreateDirectory(logDirectory);
var logFilePath = Path.Combine(logDirectory, "system-log.jsonl");
builder.Logging.AddProvider(new SystemFileLoggerProvider(logFilePath));

ExcelPackage.License.SetNonCommercialPersonal("Your Name or Organization's Name");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddFusionCache();

// Add infrastructure services (DbContext, Cloudinary, Traffic Violation Service, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add application services (CQRS handlers, etc.)
builder.Services.AddApplication();

// Add HttpContextAccessor for audit fields
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ILogStore, FileLogStore>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
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
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ChargePoint.CarManagement.Middleware.MaintenanceMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cars}/{action=Index}/{id?}");

// Sau khi build app, trước app.Run()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // Tự động apply pending migrations
}

app.Run();

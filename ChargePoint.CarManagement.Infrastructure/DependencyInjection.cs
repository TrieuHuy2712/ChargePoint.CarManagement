using ChargePoint.CarManagement.Application.Interfaces;
using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Application.Interfaces.TireService;
using ChargePoint.CarManagement.Domain.Models.TrafficViolation;
using ChargePoint.CarManagement.Infrastructure.Persistence;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using ChargePoint.CarManagement.Infrastructure.Services;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChargePoint.CarManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Cloudinary Service
            services.AddScoped<IImageUploadService, CloudinaryService>();
            services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));

            // Add Traffic Violation Service
            services.Configure<TrafficViolationSettings>(configuration.GetSection("TrafficViolation"));
            services.AddHttpClient<ITrafficViolationService, TrafficViolationService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            });

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var serverVersion = ServerVersion.AutoDetect(connectionString);
                options.UseMySql(connectionString, serverVersion);
            });

            // Add Tire Services
            services.AddScoped<ITireDraftService, TireDraftService>();
            services.AddScoped<ITireImageService, TireImageService>();

            // Add Authentication Service
            services.AddScoped<IAuthenService, AuthenService>();


            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

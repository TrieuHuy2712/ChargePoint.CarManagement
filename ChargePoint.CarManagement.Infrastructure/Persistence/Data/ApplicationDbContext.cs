using ChargePoint.CarManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChargePoint.CarManagement.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarMedia> CarMedias { get; set; }
        public DbSet<TrafficViolationCheck> TrafficViolationChecks { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<TireRecord> TireRecords { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }


        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Car || e.Entity is CarMedia || e.Entity is MaintenanceRecord || e.Entity is TireRecord)
                .ToList();
            var now = DateTime.Now;
            var currentUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Car car)
                    {
                        car.NgayTao = now;
                        car.NguoiTao = currentUserName;
                    }
                    else if (entry.Entity is CarMedia media)
                    {
                        media.CreatedAt = now;
                    }
                    else if (entry.Entity is MaintenanceRecord maintenance)
                    {
                        maintenance.NgayTao = now;
                        maintenance.NguoiTao = currentUserName;
                    }
                    else if (entry.Entity is TireRecord tire)
                    {
                        tire.NgayTao = now;
                        tire.NgayCapNhat = now;
                        tire.NguoiTao = currentUserName;
                        
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Car car)
                    {
                        car.NgayCapNhat = now;
                        car.NguoiCapNhat = currentUserName;
                    }
                    else if (entry.Entity is MaintenanceRecord maintenance)
                    {
                        maintenance.NgayCapNhat = now;
                        maintenance.NguoiCapNhat = currentUserName;
                    }
                    else if (entry.Entity is TireRecord tire)
                    {
                        tire.NgayCapNhat = now;
                        tire.NguoiCapNhat = currentUserName;
                    }
                }
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình relationship cho CarMedia
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Media)
                .WithOne(m => m.Car)
                .HasForeignKey(m => m.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình relationship cho MaintenanceRecord
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Car)
                .WithMany()
                .HasForeignKey(m => m.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình relationship cho TireRecord
            modelBuilder.Entity<TireRecord>()
                .HasOne(t => t.Car)
                .WithMany()
                .HasForeignKey(t => t.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for search optimization
            modelBuilder.Entity<Car>()
                .HasIndex(c => c.BienSo)
                .HasDatabaseName("idx_cars_bienso");

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.TenXe)
                .HasDatabaseName("idx_cars_tenxe");

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.TenKhachHang)
                .HasDatabaseName("idx_cars_tenkhachhang");

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.SoVIN)
                .HasDatabaseName("idx_cars_sovin");

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.MauXe)
                .HasDatabaseName("idx_cars_mauxe");

            modelBuilder.Entity<Car>()
                .HasIndex(c => c.Stt)
                .HasDatabaseName("idx_cars_stt");

            // Seed some sample data
            modelBuilder.Entity<Car>().HasData(
                new Car
                {
                    Id = 1,
                    Stt = 1,
                    TenXe = "VinFast VF8",
                    SoLuong = 5,
                    MauXe = "Đỏ",
                    SoVIN = "LVSHCAMB1NE000001",
                    BienSo = "30A-12345",
                    TenKhachHang = "Nguyễn Văn A",
                    ThongTinChoThue = "Xe cho thuê dài hạn",
                    OdoXe = 15000,
                    NgayTao = DateTime.Now
                },
                new Car
                {
                    Id = 2,
                    Stt = 2,
                    TenXe = "VinFast VF9",
                    SoLuong = 3,
                    MauXe = "Trắng",
                    SoVIN = "LVSHCAMB2NE000002",
                    BienSo = "30B-67890",
                    TenKhachHang = "Trần Thị B",
                    ThongTinChoThue = "Xe cho thuê ngắn hạn",
                    OdoXe = 8000,
                    NgayTao = DateTime.Now
                }
            );

            // Seed System settings
            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { Key = SystemSettingKeys.AutoUpdateOdo_Tire, Value = "false", Description = "Tự động cập nhật ODO của xe khi thêm hồ sơ lốp", Type = "boolean" },
                new SystemSetting { Key = SystemSettingKeys.AutoUpdateOdo_Maintenance, Value = "false", Description = "Tự động cập nhật ODO của xe khi thêm hồ sơ bảo dưỡng", Type = "boolean" },
                new SystemSetting { Key = SystemSettingKeys.MaintenanceMode, Value = "false", Description = "Bảo trì hệ thống (Chỉ root account mới được truy cập)", Type = "boolean" }
            );
        }
    }
}

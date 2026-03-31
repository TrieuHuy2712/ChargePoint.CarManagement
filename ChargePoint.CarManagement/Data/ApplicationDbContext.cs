using Microsoft.EntityFrameworkCore;
using ChargePoint.CarManagement.Models;

namespace ChargePoint.CarManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<CarMedia> CarMedias { get; set; }
        public DbSet<TrafficViolationCheck> TrafficViolationChecks { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<TireRecord> TireRecords { get; set; }

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
        }
    }
}

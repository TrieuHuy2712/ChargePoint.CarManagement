using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Models
{
    /// <summary>
    /// Cấp bảo dưỡng
    /// </summary>
    public enum CapBaoDuong
    {
        [Display(Name = "Cấp 1 - Bảo dưỡng cơ bản")]
        Cap1 = 1,

        [Display(Name = "Cấp 2 - Bảo dưỡng định kỳ")]
        Cap2 = 2,

        [Display(Name = "Cấp 3 - Bảo dưỡng toàn diện")]
        Cap3 = 3
    }

    /// <summary>
    /// Hồ sơ bảo dưỡng xe
    /// </summary>
    public class MaintenanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bảo dưỡng")]
        [Display(Name = "Ngày bảo dưỡng")]
        [DataType(DataType.Date)]
        public DateTime NgayBaoDuong { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập số KM")]
        [Display(Name = "Số KM lúc bảo dưỡng")]
        [Range(0, 9999999, ErrorMessage = "Số KM phải từ 0 đến 9,999,999")]
        public int SoKmBaoDuong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn cấp bảo dưỡng")]
        [Display(Name = "Cấp bảo dưỡng")]
        public CapBaoDuong CapBaoDuong { get; set; } = CapBaoDuong.Cap1;

        [Display(Name = "Số KM bảo dưỡng tiếp theo")]
        [Range(0, 9999999, ErrorMessage = "Số KM phải từ 0 đến 9,999,999")]
        public int? SoKmBaoDuongTiepTheo { get; set; }

        [Display(Name = "Nội dung bảo dưỡng")]
        [StringLength(2000)]
        public string? NoiDungBaoDuong { get; set; }

        [Display(Name = "Chi phí (VNĐ)")]
        [Column(TypeName = "decimal(18,0)")]
        [Range(0, 999999999, ErrorMessage = "Chi phí không hợp lệ")]
        public decimal ChiPhi { get; set; }

        [Display(Name = "Nơi bảo dưỡng")]
        [StringLength(200)]
        public string? NoiBaoDuong { get; set; }

        // Hình ảnh chứng từ (có thể nhiều ảnh, lưu JSON array)
        [Display(Name = "Hình ảnh chứng từ")]
        public string? HinhAnhChungTu { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [Display(Name = "Người tạo")]
        [StringLength(100)]
        public string? NguoiTao { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        // Helper để lấy danh sách ảnh
        [NotMapped]
        public List<string> DanhSachHinhAnh
        {
            get
            {
                if (string.IsNullOrEmpty(HinhAnhChungTu))
                    return [];
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(HinhAnhChungTu) ?? [];
                }
                catch
                {
                    return [];
                }
            }
        }
    }
}

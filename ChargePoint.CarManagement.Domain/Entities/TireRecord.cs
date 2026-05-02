using ChargePoint.CarManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Domain.Entities
{
    /// <summary>
    /// Vị trí lốp xe
    /// </summary>
    public enum ViTriLop
    {
        [Display(Name = "Lốp trước trái")]
        TruocTrai = 1,

        [Display(Name = "Lốp trước phải")]
        TruocPhai = 2,

        [Display(Name = "Lốp sau trái")]
        SauTrai = 3,

        [Display(Name = "Lốp sau phải")]
        SauPhai = 4
    }

    /// <summary>
    /// Loại thao tác với lốp
    /// </summary>
    public enum LoaiThaoTacLop
    {
        [Display(Name = "Thay lốp mới")]
        ThayMoi = 1,

        [Display(Name = "Đảo lốp")]
        DaoLop = 2,

        [Display(Name = "Vá lốp")]
        VaLop = 3,

        [Display(Name = "Bơm lốp")]
        BomLop = 4,

        [Display(Name = "Cân bằng động")]
        CanBangDong = 5
    }

    /// <summary>
    /// Hồ sơ quản lý lốp xe
    /// </summary>
    public class TireRecord : BaseAuditEntity
    {
        public const int DefaultExpectedLifespanKm = 50000;
        public const int WarningThresholdKm = 1000;

        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vị trí lốp")]
        [Display(Name = "Vị trí lốp")]
        public ViTriLop ViTriLop { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại thao tác")]
        [Display(Name = "Loại thao tác")]
        public LoaiThaoTacLop LoaiThaoTac { get; set; } = LoaiThaoTacLop.ThayMoi;

        [Required(ErrorMessage = "Vui lòng chọn ngày thực hiện")]
        [Display(Name = "Ngày thực hiện")]
        [DataType(DataType.Date)]
        public DateTime NgayThucHien { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập số KM")]
        [Display(Name = "ODO lúc thay/bảo dưỡng (km)")]
        [Range(0, 9999999, ErrorMessage = "Số KM phải từ 0 đến 9,999,999")]
        public int OdoThayLop { get; set; }

        [Display(Name = "Hãng lốp")]
        [StringLength(100)]
        public string? HangLop { get; set; }

        [Display(Name = "Model lốp")]
        [StringLength(100)]
        public string? ModelLop { get; set; }

        [Display(Name = "Kích thước lốp")]
        [StringLength(50)]
        public string? KichThuocLop { get; set; }

        [Display(Name = "Số KM dự kiến thay tiếp")]
        [Range(0, 9999999)]
        public int? OdoThayTiepTheo { get; set; }

        [Display(Name = "Chi phí (VNĐ)")]
        [Column(TypeName = "decimal(18,0)")]
        [Range(0, 999999999)]
        public decimal ChiPhi { get; set; }

        [Display(Name = "Nơi thực hiện")]
        [StringLength(200)]
        public string? NoiThucHien { get; set; }

        // Hình ảnh chứng từ (JSON array)
        [Display(Name = "Hình ảnh chứng từ")]
        public string? HinhAnhChungTu { get; set; }

        // Hình ảnh DOT lốp xe (JSON array)
        [Display(Name = "Hình ảnh DOT lốp")]
        public string? HinhAnhDOT { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }

        // Helper để lấy danh sách ảnh chứng từ
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

        // Helper để lấy danh sách ảnh DOT
        [NotMapped]
        public List<string> DanhSachHinhAnhDOT
        {
            get
            {
                if (string.IsNullOrEmpty(HinhAnhDOT))
                    return [];
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<string>>(HinhAnhDOT) ?? [];
                }
                catch
                {
                    return [];
                }
            }
        }

        // Helper lấy tên vị trí lốp
        [NotMapped]
        public string TenViTriLop => ViTriLop switch
        {
            ViTriLop.TruocTrai => "Trước trái",
            ViTriLop.TruocPhai => "Trước phải",
            ViTriLop.SauTrai => "Sau trái",
            ViTriLop.SauPhai => "Sau phải",
            _ => "Không xác định"
        };

        // Helper lấy icon vị trí
        [NotMapped]
        public string IconViTri => ViTriLop switch
        {
            ViTriLop.TruocTrai => "↖️",
            ViTriLop.TruocPhai => "↗️",
            ViTriLop.SauTrai => "↙️",
            ViTriLop.SauPhai => "↘️",
            _ => "⚪"
        };
    }
}

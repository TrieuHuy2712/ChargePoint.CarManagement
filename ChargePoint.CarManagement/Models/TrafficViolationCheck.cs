using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ChargePoint.CarManagement.Models.Enums;

namespace ChargePoint.CarManagement.Models
{
    public class TrafficViolationCheck : BaseAuditEntity
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [Display(Name = "Ngày kiểm tra")]
        public DateTime NgayKiemTra { get; set; } = DateTime.Now;

        [Display(Name = "Có vi phạm")]
        public bool CoViPham { get; set; }

        [Display(Name = "Số lượng vi phạm")]
        public int SoLuongViPham { get; set; }

        [Display(Name = "Ngày giờ vi phạm")]
        public DateTime? NgayGioViPham { get; set; }

        [Display(Name = "Nội dung vi phạm")]
        [StringLength(500)]
        public string? NoiDungViPham { get; set; }

        [Display(Name = "Địa điểm vi phạm")]
        [StringLength(500)]
        public string? DiaDiemViPham { get; set; }

        [Display(Name = "Trạng thái xử lý")]
        public ViolationStatus TrangThaiXuLy { get; set; } = ViolationStatus.DaBao;

        [Display(Name = "Ngày cập nhật trạng thái")]
        public DateTime? NgayCapNhatTrangThai { get; set; }

        [Display(Name = "Người xử lý")]
        [StringLength(100)]
        public string? NguoiXuLy { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }
    }
}

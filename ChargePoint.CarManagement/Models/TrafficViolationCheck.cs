using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Models
{
    public class TrafficViolationCheck
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

        [Display(Name = "Tổng tiền phạt")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal TongTienPhat { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [Display(Name = "Người kiểm tra")]
        [StringLength(100)]
        public string? NguoiKiemTra { get; set; }
    }
}

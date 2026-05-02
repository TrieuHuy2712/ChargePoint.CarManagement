using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Models
{
    /// <summary>
    /// Lớp cơ sở chứa các trường audit chung cho các entity
    /// </summary>
    public abstract class BaseAuditEntity
    {
        [Display(Name = "Người tạo")]
        [StringLength(100)]
        public string? NguoiTao { get; set; }

        [Display(Name = "Người cập nhật")]
        [StringLength(100)]
        public string? NguoiCapNhat { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }
    }
}

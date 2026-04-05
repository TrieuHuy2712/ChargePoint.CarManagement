using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Models
{
    // Enum cho màu biển số
    public enum MauBienSo
    {
        [Display(Name = "Biển trắng")]
        Trang = 0,

        [Display(Name = "Biển vàng")]
        Vang = 1
    }

    public class Car
    {
        public int Id { get; set; }

        [Display(Name = "STT")]
        public int Stt { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên xe")]
        [Display(Name = "Tên xe")]
        [StringLength(100)]
        public string TenXe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Display(Name = "Số lượng")]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1 đến 1000")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu xe")]
        [Display(Name = "Màu xe")]
        [StringLength(50)]
        public string MauXe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số VIN")]
        [Display(Name = "Số VIN")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "Số VIN phải có 17 ký tự")]
        public string SoVIN { get; set; } = string.Empty;

        [Display(Name = "Biển số")]
        [StringLength(20)]
        public string? BienSo { get; set; }

        [Display(Name = "Màu biển số")]
        public MauBienSo MauBienSo { get; set; } = MauBienSo.Trang;

        [Display(Name = "Biển số cũ")]
        [StringLength(20)]
        public string? BienSoCu { get; set; }

        [Display(Name = "Tên khách hàng")]
        [StringLength(100)]
        public string? TenKhachHang { get; set; }

        [Display(Name = "Thông tin xe cho thuê")]
        [StringLength(500)]
        public string? ThongTinChoThue { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ODO xe")]
        [Display(Name = "ODO xe (km)")]
        [Range(0, 999999, ErrorMessage = "ODO phải từ 0 đến 999999")]
        public int OdoXe { get; set; }

        // Backward-friendly primary image URL
        [Display(Name = "Hình ảnh chính")]
        [StringLength(1000)]
        public string? PrimaryImageUrl { get; set; }

        // Navigation: all media items for this car (images)
        [Display(Name = "Media")]
        public ICollection<CarMedia>? Media { get; set; } = new List<CarMedia>();

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }
    }
}

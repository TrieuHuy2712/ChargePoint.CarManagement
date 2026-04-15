using ChargePoint.CarManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ChargePoint.CarManagement.Models.ViewModels
{
    public class CarViewModel : BaseAuditEntity
    {
        public int Id { get; set; }
        public int Stt { get; set; }

        [Display(Name = "Tên xe")]
        public string TenXe { get; set; } = string.Empty;

        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }
        [Display(Name = "Màu xe")]
        public string MauXe { get; set; } = string.Empty;
        [Display(Name = "Số VIN")]
        public string SoVIN { get; set; } = string.Empty;

        [Display(Name = "Biển số")]
        public string? BienSo { get; set; }

        [Display(Name = "Biển số cũ")]
        public string? BienSoCu { get; set; }

        [Display(Name = "Màu biển số")]
        public MauBienSo MauBienSo { get; set; } = MauBienSo.Trang;

        [Display(Name = "Tên khách hàng")]
        public string? TenKhachHang { get; set; }

        [Display(Name = "Thông tin xe cho thuê")]
        public string? ThongTinChoThue { get; set; }

        [Display(Name = "Ngày thuê")]
        [DataType(DataType.Date)]
        public DateTime? NgayThue { get; set; }

        [Display(Name = "Ngày hết hạn")]
        [DataType(DataType.Date)]
        public DateTime? NgayHetHan { get; set; }

        [Display(Name = "ODO xe")]
        public int OdoXe { get; set; }

        // Image friendly properties used by views
        public string? PrimaryImageUrl { get; set; }
        public string? FirstGsmImageUrl { get; set; }
        public string? FirstKhImageUrl { get; set; }
        public string? HinhAnhNhanBanGiaoGSM { get; set; }
        public string? HinhAnhBanGiaoKHThucTe { get; set; }
        public string? HinhAnhNhanBanGiao { get; set; }
        public string? HinhAnhBanGiaoKH { get; set; }

        // Existing media collection for Edit view
        public ICollection<CarMedia>? Media { get; set; }

        public static CarViewModel FromCar(Car car)
        {
            var vm = new CarViewModel
            {
                Id = car.Id,
                Stt = car.Stt,
                TenXe = car.TenXe,
                SoLuong = car.SoLuong,
                MauXe = car.MauXe,
                SoVIN = car.SoVIN,
                BienSo = car.BienSo,
                BienSoCu = car.BienSoCu,
                MauBienSo = car.MauBienSo,
                TenKhachHang = car.TenKhachHang,
                ThongTinChoThue = car.ThongTinChoThue,
                NgayThue = car.NgayThue,
                NgayHetHan = car.NgayHetHan,
                OdoXe = car.OdoXe,
                PrimaryImageUrl = car.PrimaryImageUrl,
                NguoiTao = car.NguoiTao,
                NguoiCapNhat = car.NguoiCapNhat,
                NgayTao = car.NgayTao,
                NgayCapNhat = car.NgayCapNhat,
                Media = car.Media
            };

            if (car.Media != null && car.Media.Any())
            {
                // Lấy ảnh đầu tiên từ nhóm thân xe GSM
                vm.FirstGsmImageUrl = car.Media.FirstOrDefault(m => m.Type.IsThanXe() && m.Type.IsGSM())?.Url;
                // Lấy ảnh đầu tiên từ nhóm thân xe KH
                vm.FirstKhImageUrl = car.Media.FirstOrDefault(m => m.Type.IsThanXe() && m.Type.IsKH())?.Url;

                if (string.IsNullOrEmpty(vm.PrimaryImageUrl))
                {
                    vm.PrimaryImageUrl = car.Media.FirstOrDefault(m => m.IsPrimary)?.Url
                                       ?? vm.FirstGsmImageUrl
                                       ?? vm.FirstKhImageUrl;
                }
            }

            // Legacy/view-friendly names
            vm.HinhAnhNhanBanGiaoGSM = vm.FirstGsmImageUrl;
            vm.HinhAnhBanGiaoKHThucTe = vm.FirstKhImageUrl;
            vm.HinhAnhNhanBanGiao = vm.HinhAnhNhanBanGiaoGSM;
            vm.HinhAnhBanGiaoKH = vm.HinhAnhBanGiaoKHThucTe;

            return vm;
        }
    }
}

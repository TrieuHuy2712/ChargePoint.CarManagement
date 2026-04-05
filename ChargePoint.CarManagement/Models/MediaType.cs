using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChargePoint.CarManagement.Models
{
    public enum MediaType
    {
        Image_Primary = -1,

        // Hình ảnh nhận xe từ GSM - Thân xe
        Image_GSM_DauXe = 100,
        Image_GSM_DuoiXe = 101,
        Image_GSM_MaVETC = 102,
        Image_GSM_TemDangKiem = 103,
        Image_GSM_GocCanTruocTrai = 104,
        Image_GSM_GocCanSauTrai = 105,
        Image_GSM_ThanXeTrai = 106,
        Image_GSM_GuongTrai = 107,
        Image_GSM_PinGocTrai = 108,
        Image_GSM_GocCanTruocPhai = 109,
        Image_GSM_GocCanSauPhai = 110,
        Image_GSM_ThanXePhai = 111,
        Image_GSM_GuongPhai = 112,
        Image_GSM_PinGocPhai = 113,
        Image_GSM_HangGheTruoc = 114,
        Image_GSM_HangGheSau = 115,

        // Hình ảnh giấy tờ xe từ GSM
        Image_GSM_GiayTo_DangKiem = 120,
        Image_GSM_GiayTo_Cavet = 121,
        Image_GSM_GiayTo_BaoHiemTNDS = 122,
        Image_GSM_GiayTo_BBNhanXe = 123,

        // Hình ảnh giao xe cho KH - Thân xe
        Image_KH_DauXe = 200,
        Image_KH_DuoiXe = 201,
        Image_KH_MaVETC = 202,
        Image_KH_TemDangKiem = 203,
        Image_KH_GocCanTruocTrai = 204,
        Image_KH_GocCanSauTrai = 205,
        Image_KH_ThanXeTrai = 206,
        Image_KH_GuongTrai = 207,
        Image_KH_PinGocTrai = 208,
        Image_KH_GocCanTruocPhai = 209,
        Image_KH_GocCanSauPhai = 210,
        Image_KH_ThanXePhai = 211,
        Image_KH_GuongPhai = 212,
        Image_KH_PinGocPhai = 213,
        Image_KH_HangGheTruoc = 214,
        Image_KH_HangGheSau = 215,

        // Hình ảnh giấy tờ xe giao KH
        Image_KH_GiayTo_DangKiem = 220,
        Image_KH_GiayTo_Cavet = 221,
        Image_KH_GiayTo_BaoHiemTNDS = 222,
        Image_KH_GiayTo_BBNhanXe = 223,

        Image_GSM = 0,
        Image_KH = 1
        // Add other media types as needed
    }

    public class CarMedia
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public MediaType Type { get; set; }

        [Required]
        [StringLength(1000)]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "Tên file")]
        [StringLength(260)]
        public string? FileName { get; set; }

        [Display(Name = "Ảnh chính")]
        public bool IsPrimary { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(CarId))]
        public Car? Car { get; set; }
    }
}


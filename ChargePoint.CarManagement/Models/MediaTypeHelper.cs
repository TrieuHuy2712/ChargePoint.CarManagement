using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Models
{
    public static class MediaTypeHelper
    {
        public static string GetDisplayName(this MediaType type)
        {
            return type switch
            {
                MediaType.Image_Primary => "Ảnh chính",

                // GSM - Thân xe
                MediaType.Image_GSM_DauXe => "Đầu xe (GSM)",
                MediaType.Image_GSM_DuoiXe => "Đuôi xe (GSM)",
                MediaType.Image_GSM_MaVETC => "Mã VETC (GSM)",
                MediaType.Image_GSM_TemDangKiem => "Tem đăng kiểm (GSM)",
                MediaType.Image_GSM_GocCanTruocTrai => "Góc cản trước trái (GSM)",
                MediaType.Image_GSM_GocCanSauTrai => "Góc cản sau trái (GSM)",
                MediaType.Image_GSM_ThanXeTrai => "Thân xe trái (GSM)",
                MediaType.Image_GSM_GuongTrai => "Gương trái (GSM)",
                MediaType.Image_GSM_PinGocTrai => "Pin góc trái (GSM)",
                MediaType.Image_GSM_GocCanTruocPhai => "Góc cản trước phải (GSM)",
                MediaType.Image_GSM_GocCanSauPhai => "Góc cản sau phải (GSM)",
                MediaType.Image_GSM_ThanXePhai => "Thân xe phải (GSM)",
                MediaType.Image_GSM_GuongPhai => "Gương phải (GSM)",
                MediaType.Image_GSM_PinGocPhai => "Pin góc phải (GSM)",
                MediaType.Image_GSM_HangGheTruoc => "Hàng ghế trước (GSM)",
                MediaType.Image_GSM_HangGheSau => "Hàng ghế sau (GSM)",

                // GSM - Giấy tờ
                MediaType.Image_GSM_GiayTo_DangKiem => "Đăng kiểm (GSM)",
                MediaType.Image_GSM_GiayTo_Cavet => "Cavet/Thế chấp NH (GSM)",
                MediaType.Image_GSM_GiayTo_BaoHiemTNDS => "Bảo hiểm TNDS (GSM)",
                MediaType.Image_GSM_GiayTo_BBNhanXe => "BB nhận xe (GSM)",

                // KH - Thân xe
                MediaType.Image_KH_DauXe => "Đầu xe (KH)",
                MediaType.Image_KH_DuoiXe => "Đuôi xe (KH)",
                MediaType.Image_KH_MaVETC => "Mã VETC (KH)",
                MediaType.Image_KH_TemDangKiem => "Tem đăng kiểm (KH)",
                MediaType.Image_KH_GocCanTruocTrai => "Góc cản trước trái (KH)",
                MediaType.Image_KH_GocCanSauTrai => "Góc cản sau trái (KH)",
                MediaType.Image_KH_ThanXeTrai => "Thân xe trái (KH)",
                MediaType.Image_KH_GuongTrai => "Gương trái (KH)",
                MediaType.Image_KH_PinGocTrai => "Pin góc trái (KH)",
                MediaType.Image_KH_GocCanTruocPhai => "Góc cản trước phải (KH)",
                MediaType.Image_KH_GocCanSauPhai => "Góc cản sau phải (KH)",
                MediaType.Image_KH_ThanXePhai => "Thân xe phải (KH)",
                MediaType.Image_KH_GuongPhai => "Gương phải (KH)",
                MediaType.Image_KH_PinGocPhai => "Pin góc phải (KH)",
                MediaType.Image_KH_HangGheTruoc => "Hàng ghế trước (KH)",
                MediaType.Image_KH_HangGheSau => "Hàng ghế sau (KH)",

                // KH - Giấy tờ
                MediaType.Image_KH_GiayTo_DangKiem => "Đăng kiểm (KH)",
                MediaType.Image_KH_GiayTo_Cavet => "Cavet/Thế chấp NH (KH)",
                MediaType.Image_KH_GiayTo_BaoHiemTNDS => "Bảo hiểm TNDS (KH)",
                MediaType.Image_KH_GiayTo_BBNhanXe => "BB nhận xe (KH)",

                _ => type.ToString()
            };
        }

        public static string GetCategory(this MediaType type)
        {
            return type switch
            {
                MediaType.Image_Primary => "Ảnh chính",

                >= MediaType.Image_GSM_DauXe and <= MediaType.Image_GSM_HangGheSau => "Hình ảnh thân xe GSM",
                >= MediaType.Image_GSM_GiayTo_DangKiem and <= MediaType.Image_GSM_GiayTo_BBNhanXe => "Giấy tờ xe GSM",

                >= MediaType.Image_KH_DauXe and <= MediaType.Image_KH_HangGheSau => "Hình ảnh thân xe KH",
                >= MediaType.Image_KH_GiayTo_DangKiem and <= MediaType.Image_KH_GiayTo_BBNhanXe => "Giấy tờ xe KH",

                _ => "Khác"
            };
        }

        public static bool IsGSM(this MediaType type)
        {
            return type >= MediaType.Image_GSM_DauXe && type <= MediaType.Image_GSM_GiayTo_BBNhanXe;
        }

        public static bool IsKH(this MediaType type)
        {
            return type >= MediaType.Image_KH_DauXe && type <= MediaType.Image_KH_GiayTo_BBNhanXe;
        }

        public static bool IsGiayTo(this MediaType type)
        {
            return (type >= MediaType.Image_GSM_GiayTo_DangKiem && type <= MediaType.Image_GSM_GiayTo_BBNhanXe) ||
                   (type >= MediaType.Image_KH_GiayTo_DangKiem && type <= MediaType.Image_KH_GiayTo_BBNhanXe);
        }

        public static bool IsThanXe(this MediaType type)
        {
            return (type >= MediaType.Image_GSM_DauXe && type <= MediaType.Image_GSM_HangGheSau) ||
                   (type >= MediaType.Image_KH_DauXe && type <= MediaType.Image_KH_HangGheSau);
        }

        // Các nhóm hình ảnh thân xe GSM
        public static List<MediaType> GetGSMThanXeTypes()
        {
            return new List<MediaType>
            {
                MediaType.Image_GSM_DauXe,
                MediaType.Image_GSM_DuoiXe,
                MediaType.Image_GSM_MaVETC,
                MediaType.Image_GSM_TemDangKiem,
                MediaType.Image_GSM_GocCanTruocTrai,
                MediaType.Image_GSM_GocCanSauTrai,
                MediaType.Image_GSM_ThanXeTrai,
                MediaType.Image_GSM_GuongTrai,
                MediaType.Image_GSM_PinGocTrai,
                MediaType.Image_GSM_GocCanTruocPhai,
                MediaType.Image_GSM_GocCanSauPhai,
                MediaType.Image_GSM_ThanXePhai,
                MediaType.Image_GSM_GuongPhai,
                MediaType.Image_GSM_PinGocPhai,
                MediaType.Image_GSM_HangGheTruoc,
                MediaType.Image_GSM_HangGheSau
            };
        }

        // Các nhóm giấy tờ GSM
        public static List<MediaType> GetGSMGiayToTypes()
        {
            return new List<MediaType>
            {
                MediaType.Image_GSM_GiayTo_DangKiem,
                MediaType.Image_GSM_GiayTo_Cavet,
                MediaType.Image_GSM_GiayTo_BaoHiemTNDS,
                MediaType.Image_GSM_GiayTo_BBNhanXe
            };
        }

        // Các nhóm hình ảnh thân xe KH
        public static List<MediaType> GetKHThanXeTypes()
        {
            return new List<MediaType>
            {
                MediaType.Image_KH_DauXe,
                MediaType.Image_KH_DuoiXe,
                MediaType.Image_KH_MaVETC,
                MediaType.Image_KH_TemDangKiem,
                MediaType.Image_KH_GocCanTruocTrai,
                MediaType.Image_KH_GocCanSauTrai,
                MediaType.Image_KH_ThanXeTrai,
                MediaType.Image_KH_GuongTrai,
                MediaType.Image_KH_PinGocTrai,
                MediaType.Image_KH_GocCanTruocPhai,
                MediaType.Image_KH_GocCanSauPhai,
                MediaType.Image_KH_ThanXePhai,
                MediaType.Image_KH_GuongPhai,
                MediaType.Image_KH_PinGocPhai,
                MediaType.Image_KH_HangGheTruoc,
                MediaType.Image_KH_HangGheSau
            };
        }

        // Các nhóm giấy tờ KH
        public static List<MediaType> GetKHGiayToTypes()
        {
            return new List<MediaType>
            {
                MediaType.Image_KH_GiayTo_DangKiem,
                MediaType.Image_KH_GiayTo_Cavet,
                MediaType.Image_KH_GiayTo_BaoHiemTNDS,
                MediaType.Image_KH_GiayTo_BBNhanXe
            };
        }

        // Lấy tất cả các loại hình ảnh theo nhóm
        public static Dictionary<string, List<MediaType>> GetAllGroupedTypes()
        {
            return new Dictionary<string, List<MediaType>>
            {
                { "Hình ảnh thân xe GSM", GetGSMThanXeTypes() },
                { "Giấy tờ xe GSM", GetGSMGiayToTypes() },
                { "Hình ảnh thân xe KH", GetKHThanXeTypes() },
                { "Giấy tờ xe KH", GetKHGiayToTypes() }
            };
        }
    }
}

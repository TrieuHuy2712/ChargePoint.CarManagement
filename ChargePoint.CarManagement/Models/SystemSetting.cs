using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Models
{
    public static class SystemSettingKeys
    {
        public const string AutoUpdateOdo_Tire = "AutoUpdateOdo_Tire";
        public const string AutoUpdateOdo_Maintenance = "AutoUpdateOdo_Maintenance";
        public const string MaintenanceMode = "MaintenanceMode";
    }

    public class SystemSetting
    {
        [Key]
        [StringLength(50)]
        [Display(Name = "Mã thiết lập")]
        public string Key { get; set; } = string.Empty;

        [Display(Name = "Giá trị")]
        public string? Value { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Kiểu dữ liệu")]
        public string Type { get; set; } = "boolean"; // boolean, string, number
    }
}

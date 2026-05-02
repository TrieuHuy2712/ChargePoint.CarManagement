using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Domain.Enums
{
    public enum DocumentType
    {
        [Display(Name = "Bảo dưỡng")] BaoDuong = 0,
        [Display(Name = "Sửa chữa")] SuaChua = 1,
        [Display(Name = "Sửa chữa - Bảo dưỡng")] SuaChuaBaoDuong = 2
    }
}

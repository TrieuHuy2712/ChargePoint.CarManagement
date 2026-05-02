using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Domain.Enums
{
    public enum ViolationStatus
    {
        [Display(Name = "Đã báo")]
        DaBao = 0,

        [Display(Name = "Đang chờ xử lý")]
        DangChoXuLy = 1,

        [Display(Name = "Đang xử lý")]
        DangXuLy = 2,

        [Display(Name = "Đã xử lý")]
        DaXuLy = 3
    }
}

using System.ComponentModel.DataAnnotations;

namespace ChargePoint.CarManagement.Domain.Enums
{
    /// <summary>
    /// Cấp bảo dưỡng
    /// </summary>
    public enum MaintenanceLevel
    {
        [Display(Name = "Cấp 1 - Bảo dưỡng cơ bản")]
        Cap1 = 1,

        [Display(Name = "Cấp 2 - Bảo dưỡng định kỳ")]
        Cap2 = 2,

        [Display(Name = "Cấp 3 - Bảo dưỡng toàn diện")]
        Cap3 = 3
    }
}

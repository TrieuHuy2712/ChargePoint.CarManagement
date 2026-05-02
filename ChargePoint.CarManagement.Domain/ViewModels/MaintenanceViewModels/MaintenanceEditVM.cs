using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels
{
    public class MaintenanceEditVM : CarVM
    {
        public MaintenanceRecord? Record { get; set; }
    }
}

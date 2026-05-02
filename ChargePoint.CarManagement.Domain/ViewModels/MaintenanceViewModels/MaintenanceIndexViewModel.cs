using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels
{
    public class MaintenanceIndexViewModel : CarVM
    {
        public MaintenanceRecord? LastMaintenance { get; set; }
        public int TotalMaintenances { get; set; }

    }
}

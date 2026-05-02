using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels
{
    public class MaintenanceHistoryViewModel : CarVM
    {
        public List<MaintenanceRecord>? MaintenanceRecords { get; set; }
    }
}

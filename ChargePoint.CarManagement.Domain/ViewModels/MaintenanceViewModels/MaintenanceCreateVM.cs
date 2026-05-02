using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChargePoint.CarManagement.Domain.ViewModels.MaintenanceViewModels
{
    public class MaintenanceCreateVM
    {
        public SelectList? SelectListCars { get; set; }
        public List<Car>? Cars { get; set; }
        public required MaintenanceRecord MaintenanceRecord { get; set; }
        public DocumentType LoaiHoSo { get; set; } = DocumentType.BaoDuong;

    }
}

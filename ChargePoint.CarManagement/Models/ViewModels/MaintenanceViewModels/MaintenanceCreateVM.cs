using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChargePoint.CarManagement.Models.ViewModels.MaintenanceViewModels
{
    public class MaintenanceCreateVM
    {
        public SelectList? SelectListCars { get; set; }
        public List<Car>? Cars { get; set; }
        public required MaintenanceRecord MaintenanceRecord { get; set; }
        public LoaiHoSo LoaiHoSo { get; set; } = LoaiHoSo.BaoDuong;

    }
}

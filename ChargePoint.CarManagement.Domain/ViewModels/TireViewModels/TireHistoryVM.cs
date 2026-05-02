using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TireViewModels
{
    public class TireHistoryVM : CarVM
    {
        public ViTriLop? ViTriLop { get; set; }
        public List<TireRecord>? Records { get; set; }
    }
}

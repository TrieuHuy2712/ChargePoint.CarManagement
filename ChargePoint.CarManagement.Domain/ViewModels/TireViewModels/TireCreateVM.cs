using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TireViewModels
{
    public class TireCreateVM
    {
        public Car? Car { get; set; }
        public TireRecord? TireRecord { get; set; }
    }
}

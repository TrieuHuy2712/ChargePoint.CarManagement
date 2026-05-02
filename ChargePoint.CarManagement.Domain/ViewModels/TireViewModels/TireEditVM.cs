using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TireViewModels
{
    public class TireEditVM : CarVM
    {
        public TireRecord? Record { get; set; }
    }
}

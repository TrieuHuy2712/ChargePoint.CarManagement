using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels
{
    public class TrafficViolationIndexVM : CarVM
    {
        public TrafficViolationCheck? LastCheck { get; set; }
    }
}

using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TrafficViolationViewModels
{
    public class TrafficViolationHistoryVM : CarVM
    {
        public List<TrafficViolationCheck>? Checks { get; set; }
    }
}

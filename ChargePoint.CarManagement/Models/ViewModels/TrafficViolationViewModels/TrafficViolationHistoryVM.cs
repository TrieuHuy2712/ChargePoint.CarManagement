namespace ChargePoint.CarManagement.Models.ViewModels.TrafficViolationViewModels
{
    public class TrafficViolationHistoryVM : CarVM
    {
        public List<TrafficViolationCheck>? Checks { get; set; }
    }
}

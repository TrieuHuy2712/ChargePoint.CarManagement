namespace ChargePoint.CarManagement.Models.ViewModels.TireViewModels
{
    public class TireHistoryVM : CarVM
    {
        public ViTriLop? ViTriLop { get; set; }
        public List<TireRecord>? Records { get; set; }
    }
}

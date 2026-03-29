namespace ChargePoint.CarManagement.Models.ViewModels.TireViewModels
{
    public class TireIndexViewModel : CarVM
    {
        public List<TireRecordDetailIndexVM> TireRecords { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TireRecordDetailIndexVM
    {
        public ViTriLop ViTri { get; set; }
        public TireRecord? LastRecord { get; set; }
    }
}

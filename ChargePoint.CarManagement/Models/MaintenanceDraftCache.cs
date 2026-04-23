namespace ChargePoint.CarManagement.Models
{
    public class MaintenanceDraftCache
    {
        public MaintenanceRecord? MaintenanceRecord { get; set; }
        public List<CachedFileData> HinhAnhChungTuFiles { get; set; } = [];
    }
}

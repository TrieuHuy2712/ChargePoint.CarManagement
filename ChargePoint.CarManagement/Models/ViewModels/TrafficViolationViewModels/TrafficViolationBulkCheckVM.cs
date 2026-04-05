namespace ChargePoint.CarManagement.Models.ViewModels.TrafficViolationViewModels
{
    public class TrafficViolationBulkCheckVM
    {
        public int CarId { get; set; }
        public string? BienSo { get; set; }
        public int SoLuongViPham { get; set; }
        public DateTime? NgayGioViPham { get; set; }
        public string? NoiDungViPham { get; set; }
        public string? DiaDiemViPham { get; set; }
        public string? GhiChu { get; set; }
    }
}

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
        public string? TrangThaiCSGT { get; set; }
        public string? FullViPhamData { get; set; }
        public string? GhiChu { get; set; }
        public bool IsError { get; set; }
        public string? ErrorMessage { get; set; }
        public List<ChargePoint.CarManagement.Models.ViolationDetail>? DanhSachViPham { get; set; }
    }
}

namespace ChargePoint.CarManagement.Domain.Models.TrafficViolation
{
    public class CarCheckResult
    {
        public int CarId { get; set; }
        public string? BienSo { get; set; }
        public string TenXe { get; set; }
        public bool Success { get; set; }
        public bool CoViPham { get; set; }
        public int SoLuongViPham { get; set; }
        public List<ViolationDetail> DanhSachViPham { get; set; }
        public string? Message { get; set; }
    }
}

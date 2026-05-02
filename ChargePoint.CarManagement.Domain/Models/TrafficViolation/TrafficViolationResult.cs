namespace ChargePoint.CarManagement.Domain.Models.TrafficViolation
{
    public class TrafficViolationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? BienSo { get; set; }
        public bool CoViPham { get; set; }
        public int SoLuongViPham { get; set; }
        public List<ViolationDetail> DanhSachViPham { get; set; } = [];
    }

    public class ViolationDetail
    {
        public string? MaViPham { get; set; }
        public string? NgayViPham { get; set; }
        public string? DiaDiem { get; set; }
        public string? HanhVi { get; set; }
        public string? TrangThai { get; set; }
        public decimal SoTienPhat { get; set; }
        public string? DonViPhatHien { get; set; }
        public string? NoiGiaiQuyet { get; set; }
        public string? LoaiPhuongTien { get; set; }
        public string? MauBien { get; set; }
    }
}

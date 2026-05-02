using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Application.Tire.Models;

public class TirePositionDraft
{
    public LoaiThaoTacLop LoaiThaoTac { get; set; }
    public DateTime NgayThucHien { get; set; }
    public int OdoThayLop { get; set; }
    public string? HangLop { get; set; }
    public string? ModelLop { get; set; }
    public string? KichThuocLop { get; set; }
    public int? OdoThayTiepTheo { get; set; }
    public decimal ChiPhi { get; set; }
    public string? NoiThucHien { get; set; }
    public string? GhiChu { get; set; }
}

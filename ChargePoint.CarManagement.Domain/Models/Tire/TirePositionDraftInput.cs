using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePoint.CarManagement.Domain.Models.Tire
{
    public sealed class TirePositionDraftInput
    {
        public int LoaiThaoTac { get; set; }
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
}

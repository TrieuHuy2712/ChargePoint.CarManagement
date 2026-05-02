using ChargePoint.CarManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePoint.CarManagement.Domain.Mapping
{
    public class TireMapping
    {
        public static TireRecord CloneTireRecordForPosition(TireRecord source, ViTriLop position)
        {
            return new TireRecord
            {
                CarId = source.CarId,
                ViTriLop = position,
                LoaiThaoTac = source.LoaiThaoTac,
                NgayThucHien = source.NgayThucHien,
                OdoThayLop = source.OdoThayLop,
                HangLop = source.HangLop,
                ModelLop = source.ModelLop,
                KichThuocLop = source.KichThuocLop,
                OdoThayTiepTheo = source.OdoThayTiepTheo,
                ChiPhi = source.ChiPhi,
                NoiThucHien = source.NoiThucHien,
                GhiChu = source.GhiChu,
                HinhAnhChungTu = source.HinhAnhChungTu,
                HinhAnhDOT = source.HinhAnhDOT
            };
        }
    }
}

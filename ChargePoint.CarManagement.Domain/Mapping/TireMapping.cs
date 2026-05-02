using ChargePoint.CarManagement.Application.Tire.Models;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Domain.Models.Tire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public static Dictionary<ViTriLop, TirePositionDraft> ParsePositionDrafts(string? positionDraftsJson)
        {
            if (string.IsNullOrWhiteSpace(positionDraftsJson))
            {
                return [];
            }

            try
            {
                var raw = JsonSerializer.Deserialize<Dictionary<string, TirePositionDraftInput>>(
                    positionDraftsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                if (raw == null || raw.Count == 0)
                {
                    return [];
                }

                var result = new Dictionary<ViTriLop, TirePositionDraft>();
                foreach (var item in raw)
                {
                    if (!int.TryParse(item.Key, out var positionInt) || !Enum.IsDefined(typeof(ViTriLop), positionInt))
                    {
                        continue;
                    }

                    var position = (ViTriLop)positionInt;
                    var draft = item.Value;
                    result[position] = new TirePositionDraft
                    {
                        LoaiThaoTac = Enum.IsDefined(typeof(LoaiThaoTacLop), draft.LoaiThaoTac)
                            ? (LoaiThaoTacLop)draft.LoaiThaoTac
                            : LoaiThaoTacLop.ThayMoi,
                        NgayThucHien = draft.NgayThucHien == default ? DateTime.Now : draft.NgayThucHien,
                        OdoThayLop = draft.OdoThayLop,
                        HangLop = draft.HangLop,
                        ModelLop = draft.ModelLop,
                        KichThuocLop = draft.KichThuocLop,
                        OdoThayTiepTheo = draft.OdoThayTiepTheo,
                        ChiPhi = draft.ChiPhi,
                        NoiThucHien = draft.NoiThucHien,
                        GhiChu = draft.GhiChu
                    };
                }

                return result;
            }
            catch
            {
                return [];
            }
        }
    }
}

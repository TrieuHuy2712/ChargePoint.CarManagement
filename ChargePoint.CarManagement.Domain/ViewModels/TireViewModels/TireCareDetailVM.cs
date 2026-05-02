using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Domain.ViewModels.TireViewModels
{
    public class TireStatusInfo
    {
        public string BorderClass { get; set; } = string.Empty;
        public string TextClass { get; set; } = string.Empty;
        public int ExpectedNextOdo { get; set; }
        public int DrivenKm { get; set; }
        public int RemainingKm { get; set; }
        public bool IsError { get; set; }
    }

    public class TireCareDetailVM : CarVM
    {
        public List<TireRecordDetailIndexVM> TireRecordsPosition { get; set; } = new();

        public TireStatusInfo? GetTireStatus(TireRecord? record)
        {
            if (record == null) return null;

            int currentCarOdo = Car?.OdoXe ?? 0;
            int odoBase = record.OdoThayLop;
            int target = record.OdoThayTiepTheo ?? odoBase + TireRecord.DefaultExpectedLifespanKm;

            var status = new TireStatusInfo
            {
                ExpectedNextOdo = target,
                DrivenKm = currentCarOdo - odoBase,
                RemainingKm = target - currentCarOdo,
                IsError = odoBase > currentCarOdo
            };

            if (status.IsError || currentCarOdo >= target)
            {
                status.TextClass = "text-danger fw-bold";
                status.BorderClass = "status-danger";
            }
            else if (target - currentCarOdo <= TireRecord.WarningThresholdKm)
            {
                status.TextClass = "text-warning fw-bold";
                status.BorderClass = "status-warning";
            }
            else
            {
                status.TextClass = "text-success";
                status.BorderClass = "status-success";
            }

            return status;
        }
    }
}

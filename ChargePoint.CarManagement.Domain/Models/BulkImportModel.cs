namespace ChargePoint.CarManagement.Domain.Models
{
    public class BulkImportModel
    {
        public bool Success { get; set; }
        public int AddedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<string> SkippedRows { get; set; }
        public object DuplicatedVins { get; set; }
        public object DuplicatedVinRows { get; set; }
    }
}

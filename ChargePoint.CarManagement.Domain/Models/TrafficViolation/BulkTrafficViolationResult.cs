namespace ChargePoint.CarManagement.Domain.Models.TrafficViolation
{
    public class BulkTrafficViolationResult
    {
        public bool Success { get; set; }
        public int SavedCount { get; set; }
        public int TotalCount { get; set; }
        public List<string> Errors { get; set; }
        public string Message { get; set; }
    }
}

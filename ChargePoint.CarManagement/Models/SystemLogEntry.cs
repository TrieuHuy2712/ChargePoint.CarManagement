namespace ChargePoint.CarManagement.Models;

public class SystemLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "Information";
    public string Source { get; set; } = "Application";
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? UserName { get; set; }
    public string? TraceId { get; set; }
}

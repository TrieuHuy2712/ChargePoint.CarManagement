namespace ChargePoint.CarManagement.Models;

public class SystemLogIndexViewModel
{
    public IReadOnlyList<SystemLogEntry> Items { get; set; } = [];
    public string? Q { get; set; }
    public string? Level { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }

    public int TotalPages => TotalCount <= 0
        ? 1
        : (int)Math.Ceiling((double)TotalCount / PageSize);
}

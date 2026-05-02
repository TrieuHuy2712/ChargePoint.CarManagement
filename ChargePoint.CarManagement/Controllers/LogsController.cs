using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Services.Logging;
using ChargePoint.CarManagement.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePoint.CarManagement.Controllers;

[Authorize(Roles = AppRoles.RootAdmin)]
public class LogsController(ILogStore logStore) : Controller
{
    private readonly ILogStore _logStore = logStore;

    public async Task<IActionResult> Index(string? q, string? level, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize is < 10 or > 200)
        {
            pageSize = 50;
        }

        var allLogs = await _logStore.GetAllAsync(cancellationToken);

        IEnumerable<SystemLogEntry> filtered = allLogs
            .Where(x => !string.Equals(x.Source, "Request", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(level))
        {
            filtered = filtered.Where(x => x.Level.Equals(level, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            filtered = filtered.Where(x =>
                x.Message.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (x.Detail?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.UserName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.Source?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (fromDate.HasValue)
        {
            var fromUtc = fromDate.Value.Date;
            filtered = filtered.Where(x => x.TimestampUtc >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var toUtcExclusive = toDate.Value.Date.AddDays(1);
            filtered = filtered.Where(x => x.TimestampUtc < toUtcExclusive);
        }

        var totalCount = filtered.Count();
        var items = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var model = new SystemLogIndexViewModel
        {
            Items = items,
            Q = q,
            Level = level,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return View(model);
    }
}

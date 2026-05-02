using ChargePoint.CarManagement.Models;

namespace ChargePoint.CarManagement.Services.Logging;

public interface ILogStore
{
    Task WriteAsync(SystemLogEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemLogEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}

using ChargePoint.CarManagement.Domain.Entities;

namespace ChargePoint.CarManagement.Application.Interfaces.Common
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<TireRecord> TireRecords { get; }
        IGenericRepository<MaintenanceRecord> MaintenanceRecords { get; }
        IGenericRepository<Domain.Entities.Car> Cars { get; }
        IGenericRepository<Domain.Entities.SystemSetting> SystemSettings { get; }
        IGenericRepository<CarMedia> CarMedia { get; }
        IGenericRepository<TrafficViolationCheck> TrafficViolations { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}

using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Domain.Entities;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using ChargePoint.CarManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChargePoint.CarManagement.Infrastructure.Persistence
{
    public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction; 
        public IGenericRepository<TireRecord> TireRecords { get; } = new GenericRepository<TireRecord>(context);

        public IGenericRepository<MaintenanceRecord> MaintenanceRecords { get; } = new GenericRepository<MaintenanceRecord>(context);

        public IGenericRepository<TrafficViolationCheck> TrafficViolations { get; } = new GenericRepository<TrafficViolationCheck>(context);

        public IGenericRepository<Car> Cars { get; } = new GenericRepository<Car>(context);

        public IGenericRepository<CarMedia> CarMedia { get; } = new GenericRepository<CarMedia>(context);

        public IGenericRepository<SystemSetting> SystemSettings { get; } = new GenericRepository<SystemSetting>(context);
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null) throw new InvalidOperationException("No active transaction.");
            await _transaction.CommitAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null) throw new InvalidOperationException("No active transaction.");
            await _transaction.RollbackAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}

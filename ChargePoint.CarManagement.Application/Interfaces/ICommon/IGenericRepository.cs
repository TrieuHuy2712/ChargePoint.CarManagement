using System.Linq.Expressions;

namespace ChargePoint.CarManagement.Application.Interfaces.Common
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IQueryable<T>> AsQueryable();
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(
            int id,
            Func<IQueryable<T>, IQueryable<T>>? includeBuilder = null,
            CancellationToken cancellationToken = default);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);

        // Filter qua Expression — không phụ thuộc EF, vẫn dễ dùng
        Task<List<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? includeBuilder = null,
            CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}

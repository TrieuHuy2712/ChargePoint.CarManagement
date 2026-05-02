using ChargePoint.CarManagement.Application.Interfaces.Common;
using ChargePoint.CarManagement.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChargePoint.CarManagement.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
    {
        protected readonly DbSet<T> _dbSet = context.Set<T>();
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public Task<IQueryable<T>> AsQueryable()
        {
            return Task.FromResult(_dbSet.AsNoTracking().AsQueryable());
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? includeBuilder = null,
            CancellationToken cancellationToken = default
            )
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            if (includeBuilder != null)
                query = includeBuilder(query);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync([id], cancellationToken);
        }

        public async Task<T?> GetByIdAsync(
            int id,
            Func<IQueryable<T>, IQueryable<T>>? includeBuilder = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (includeBuilder != null)
                query = includeBuilder(query);

            // Assuming T has an "Id" property
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);

            return await query.FirstOrDefaultAsync(lambda, cancellationToken);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }
    }
}

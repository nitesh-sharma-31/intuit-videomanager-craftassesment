using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VideoManager.Shared.Interfaces;

namespace VideoManager.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly VideoManagerDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(VideoManagerDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

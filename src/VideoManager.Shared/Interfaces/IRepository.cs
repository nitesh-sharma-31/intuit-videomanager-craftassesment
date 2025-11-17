using System.Linq.Expressions;

namespace VideoManager.Shared.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task SaveChangesAsync();
    }
}

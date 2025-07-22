using Concord.Domain.Models;
using System.Linq.Expressions;


namespace Concord.Domain.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(Guid id);
        IQueryable<T> Query();
        Task<IEnumerable<T>> GetAllAsync(string includs = "", Expression<Func<T, bool>>? predicate = null);
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> predicate, string includs = "");

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
        Task DeleteManyAsync(IEnumerable<T> entities);
        Task SaveChangesAsync();

        Task<IEnumerable<T>> GetAllWithFilterAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageNumber = null,
        int? pageSize = null
        );
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null, string includes = null);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);

        void EnableTracking();
        void DisableTracking();

    }
}

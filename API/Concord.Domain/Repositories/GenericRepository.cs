using Concord.Domain.Context.Application;
using Concord.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Concord.Domain.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Method to get an entity by its ID  
        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Method to get all entities  
        public async Task<IEnumerable<T>> GetAllAsync(string includes = "", Expression<Func<T, bool>>? predicate = null)
        {
            var dbSet = _dbSet.AsQueryable();

            if (predicate != null)
            {
                dbSet = dbSet.Where(predicate);
            }

            if (!string.IsNullOrEmpty(includes))
            {
                var includeArray = includes.Split(',');

                foreach (var includeItem in includeArray)
                {
                    dbSet = dbSet.Include(includeItem);
                }
            }

            return await dbSet.ToListAsync();
        }

        // Method to add a new entity  
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }


        // Method to add a range of  entities  
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Method to update an existing entity  
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities); // Update the entities
        }

        // Method to delete an entity  
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        // Implement DeleteManyAsync
        public async Task DeleteManyAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        // Method to save changes to the database  
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<T> GetFirstOrDefault(Expression<Func<T, bool>> predicate, string includs = "")
        {
            var dbSet = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(includs))
            {
                var includeArray = includs.Split(',');

                foreach (var includeItem in includeArray)
                {
                    dbSet = dbSet.Include(includeItem);
                }
            }

            return await dbSet.FirstOrDefaultAsync(predicate);
        }

        public IQueryable<T> Query()
        {
            return _dbSet;
        }


        // How to use :
        // filter: var users = await _userRepository.GetAllAsync(u => u.Age > 25);
        // order: var users = await _userRepository.GetAllAsync(orderBy: u => u.OrderBy(user => user.Name));
        // pagination: var users = await _userRepository.GetAllAsync(pageNumber: 2, pageSize: 10);
        public async Task<IEnumerable<T>> GetAllWithFilterAsync(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageNumber = null,
        int? pageSize = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            // pageNumber ==> Skip
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null, string includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrEmpty(includes))
            {
                var includeArray = includes.Split(',');

                foreach (var includeItem in includeArray)
                {
                    query = query.Include(includeItem);
                }
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }

        // Disable tracking
        public void DisableTracking()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        // Enable tracking
        public void EnableTracking()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

    }
}

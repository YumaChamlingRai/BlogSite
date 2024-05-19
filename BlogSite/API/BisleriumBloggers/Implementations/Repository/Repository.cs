using System.Linq.Expressions;
using BisleriumBloggers.Persistence;
using Microsoft.EntityFrameworkCore;
using BisleriumBloggers.Abstractions.Repository;

namespace BisleriumBloggers.Implementations.Repository
{
    public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T> where T : class
    {
        private DbSet<T> _dbSet = dbContext.Set<T>();
        
        private static readonly char[] separator = [','];
        private static readonly char[] separatorArray = [','];

        public T GetFirstOrDefault(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            query = query.Where(filter!);

            if (includeProperties != null)
            {
                query = includeProperties
                    .Split(separatorArray, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            return query.FirstOrDefault()!;
        }

        public T Get(int id)
        {
            return _dbSet.Find(id)!;
        }

        public List<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }

            if (includeProperties == null) return query.ToList();
            
            query = includeProperties
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return query.ToList();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
            SaveChanges();
        }

        public void Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            SaveChanges();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
            SaveChanges();
        }

        private void SaveChanges()
        {
            dbContext.SaveChanges();
        }
    }
}


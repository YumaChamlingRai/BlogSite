using System.Linq.Expressions;
using BisleriumBloggers.Models.Base;

namespace BisleriumBloggers.Abstractions.Repository
{
    public interface IRepository<T> where T : class
    {
        T GetFirstOrDefault(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        T Get(int id);

        List<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null);

        void Add(T entity);

        void Update(T entity);
        
        void Remove(T entity);
    }
}

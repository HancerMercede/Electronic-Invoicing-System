using System.Linq.Expressions;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public interface IBaseRepository<T> where T : class
{
    IQueryable<T> FindByCondiction(Expression<Func<T, bool>> expression, bool trackChanges);
    IQueryable<T> FindAllAsync(bool trackChanges);
    Task AddAsync(T entity);
    Task Update(T entity);
    Task Delete(T entity);
}
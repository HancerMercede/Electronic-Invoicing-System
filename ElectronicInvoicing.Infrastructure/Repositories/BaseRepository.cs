using System.Linq.Expressions;
using ElectronicInvoicing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public abstract class BaseRepository<T>(AppDbContext RepositoryContext):IBaseRepository<T> where T:class
{

    public IQueryable<T> FindByCondiction(Expression<Func<T, bool>> expression, bool trackChanges)=>
        !trackChanges
            ? RepositoryContext.Set<T>()
                .Where(expression).AsNoTracking()
            : RepositoryContext.Set<T>()
                .Where(expression);
    

    public IQueryable<T> FindAllAsync(bool trackChanges) =>
        !trackChanges ? RepositoryContext.Set<T>().AsNoTracking() : RepositoryContext.Set<T>();
    
    public async Task AddAsync(T entity)=> await RepositoryContext.Set<T>().AddAsync(entity);

    public async Task Update(T entity) =>await Task.FromResult(RepositoryContext.Set<T>().Update(entity));

    public async Task Delete(T entity) => await Task.FromResult(RepositoryContext.Set<T>().Remove(entity));

}
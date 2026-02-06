using System.Linq.Expressions;
using ElectronicInvoicing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public abstract class BaseRepository<T>(RepositoryContext repositoryContext):IBaseRepository<T> where T:class
{

    public IQueryable<T> FindByCondiction(Expression<Func<T, bool>> expression, bool trackChanges)=>
        !trackChanges
            ? repositoryContext.Set<T>()
                .Where(expression).AsNoTracking()
            : repositoryContext.Set<T>()
                .Where(expression);
    

    public IQueryable<T> FindAllAsync(bool trackChanges) =>
        !trackChanges ? repositoryContext.Set<T>().AsNoTracking() : repositoryContext.Set<T>();
    
    public async Task AddAsync(T entity)=> await repositoryContext.Set<T>().AddAsync(entity);

    public async Task Update(T entity) =>await Task.FromResult(repositoryContext.Set<T>().Update(entity));

    public async Task Delete(T entity) => await Task.FromResult(repositoryContext.Set<T>().Remove(entity));

}
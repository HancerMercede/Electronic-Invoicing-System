using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public class UserRepository(RepositoryContext repositoryContext) : BaseRepository<User>(repositoryContext), IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await FindByCondiction(u => u.Id == userId, false)
            .Include(u => u.Company)
            .SingleOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await FindByCondiction(u => u.Username == username, false)
            .Include(u => u.Company)
            .SingleOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await FindByCondiction(u => u.Email == email, false)
            .Include(u => u.Company)
            .SingleOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await FindByCondiction(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, false)
            .Include(u => u.Company)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> GetAllByCompanyIdAsync(Guid companyId)
    {
        return await FindByCondiction(u => u.CompanyId == companyId, false)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await AddAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        await Update(user);
    }

    public async Task DeleteAsync(User user)
    {
        await Delete(user);
    }
}

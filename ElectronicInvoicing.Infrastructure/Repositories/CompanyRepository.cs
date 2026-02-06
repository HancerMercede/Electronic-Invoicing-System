using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Context;
using ElectronicInvoicing.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public class CompanyRepository(AppDbContext repositoryContext) : BaseRepository<Company>(repositoryContext), ICompanyRepository
{
    public async Task<Company?> GetCompanyByIdAsync(Guid companyId)
    {
        return await FindByCondiction(c=>c.Id == companyId, false)
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<Company>> GetAllCompanies(bool trackChanges)
    {
        return await FindAllAsync(trackChanges)
            .OrderBy(c=>c.Name)
            .ToListAsync();
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    { 
        await AddAsync(company);
        return company;
    }

    public async Task DeleteCompanyAsync(Company company)
    {
        await  Delete(company);
    }
}
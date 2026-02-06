using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Infrastructure.Contracts;

public interface ICompanyRepository
{
    Task<Company?> GetCompanyByIdAsync(Guid companyId);
    Task<IEnumerable<Company>>GetAllCompanies(bool trackChanges);
    
    Task<Company> CreateCompanyAsync(Company company);
    
    Task DeleteCompanyAsync(Company company);
}
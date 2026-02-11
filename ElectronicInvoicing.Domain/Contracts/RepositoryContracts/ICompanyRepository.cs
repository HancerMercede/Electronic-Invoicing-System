using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.RepositoryContracts;

public interface ICompanyRepository
{
    Task<Company?> GetCompanyByIdAsync(Guid companyId);
    Task<IEnumerable<Company>>GetAllCompanies(bool trackChanges);
    
    Task<Company> CreateCompanyAsync(Company company);
    
    Task<Company> UpdateCompanyAsync(Company company);
    
    Task DeleteCompanyAsync(Company company);
}
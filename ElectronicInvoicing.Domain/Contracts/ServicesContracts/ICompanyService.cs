using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface ICompanyService
{
    Task<Company?> GetCompanyByIdAsync(Guid companyId);
    
    Task<IEnumerable<Company>>GetAllCompanies(bool trackChanges);
    
    Task<Company> CreateCompanyAsync(Company company);
    
    Task DeleteCompanyAsync(Company company);
}
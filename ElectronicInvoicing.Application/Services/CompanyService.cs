using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Application.Services;

public class CompanyService(IUnitOfWork unitOfWork) : ICompanyService
{
    public async Task<Company?> GetCompanyByIdAsync(Guid companyId)
    {
        return await unitOfWork.CompanyRepository.GetCompanyByIdAsync(companyId);
    }

    public async Task<IEnumerable<Company>> GetAllCompanies(bool trackChanges)
    {
        return  await unitOfWork.CompanyRepository.GetAllCompanies(trackChanges);
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    {
        var result = await unitOfWork.CompanyRepository.CreateCompanyAsync(company);
        await unitOfWork.SaveChanges();
        return result;
    }

    public async Task<Company> UpdateCompanyAsync(Company company)
    {
       return await unitOfWork.CompanyRepository.UpdateCompanyAsync(company);
    }

    public async Task DeleteCompanyAsync(Company company)
    {
        await unitOfWork.CompanyRepository.DeleteCompanyAsync(company);
        await unitOfWork.SaveChanges();
    }
}
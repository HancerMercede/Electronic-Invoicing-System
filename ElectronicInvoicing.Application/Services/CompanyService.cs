using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Application.Services;

public class CompanyService(IUnitOfWork unitOfWork, IEncryptionService encryptionService) : ICompanyService
{
    public async Task<Company?> GetCompanyByIdAsync(Guid companyId)
    {
        var company = await unitOfWork.CompanyRepository.GetCompanyByIdAsync(companyId);

        if (company != null)
        {
            // Decrypt sensitive data before returning
            DecryptSensitiveData(company);
        }

        return company;
    }

    public async Task<IEnumerable<Company>> GetAllCompanies(bool trackChanges)
    {
        var companies = await unitOfWork.CompanyRepository.GetAllCompanies(trackChanges);

        // Decrypt sensitive data for each company
        foreach (var company in companies)
        {
            DecryptSensitiveData(company);
        }

        return companies;
    }

    public async Task<Company> CreateCompanyAsync(Company company)
    {
        // Encrypt sensitive data before saving
        EncryptSensitiveData(company);

        var result = await unitOfWork.CompanyRepository.CreateCompanyAsync(company);
        await unitOfWork.SaveChanges();

        // Decrypt before returning to caller
        DecryptSensitiveData(result);

        return result;
    }

    public async Task<Company> UpdateCompanyAsync(Company company)
    {
        // Encrypt sensitive data before updating
        EncryptSensitiveData(company);

        var result = await unitOfWork.CompanyRepository.UpdateCompanyAsync(company);

        // Decrypt before returning to caller
        DecryptSensitiveData(result);

        return result;
    }

    public async Task DeleteCompanyAsync(Company company)
    {
        await unitOfWork.CompanyRepository.DeleteCompanyAsync(company);
        await unitOfWork.SaveChanges();
    }

    /// <summary>
    /// Encrypts sensitive company data before saving to database.
    /// </summary>
    private void EncryptSensitiveData(Company company)
    {
        if (!string.IsNullOrEmpty(company.CertificatePassword))
        {
            company.CertificatePassword = encryptionService.Encrypt(company.CertificatePassword);
        }

        if (!string.IsNullOrEmpty(company.ApiClientSecret))
        {
            company.ApiClientSecret = encryptionService.Encrypt(company.ApiClientSecret);
        }

        if (company.DigitalCertificate != null && company.DigitalCertificate.Length > 0)
        {
            company.DigitalCertificate = encryptionService.EncryptBytes(company.DigitalCertificate);
        }
    }

    /// <summary>
    /// Decrypts sensitive company data after retrieving from database.
    /// </summary>
    private void DecryptSensitiveData(Company company)
    {
        if (!string.IsNullOrEmpty(company.CertificatePassword))
        {
            company.CertificatePassword = encryptionService.Decrypt(company.CertificatePassword);
        }

        if (!string.IsNullOrEmpty(company.ApiClientSecret))
        {
            company.ApiClientSecret = encryptionService.Decrypt(company.ApiClientSecret);
        }

        if (company.DigitalCertificate != null && company.DigitalCertificate.Length > 0)
        {
            company.DigitalCertificate = encryptionService.DecryptBytes(company.DigitalCertificate);
        }
    }
}
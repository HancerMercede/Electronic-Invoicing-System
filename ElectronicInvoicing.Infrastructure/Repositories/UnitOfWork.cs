using ElectronicInvoicing.Domain.Contracts;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Infrastructure.Context;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public class UnitOfWork(RepositoryContext repositoryContext) : IUnitOfWork
{
    private readonly Lazy<ICompanyRepository> _companyRepository = new(()=> new CompanyRepository(repositoryContext));
    private readonly Lazy<IInvoiceItemRepository> _invoiceItemRepository = new(() => new InvoiceItemRepository(repositoryContext));
    private readonly Lazy<IInvoiceRepository> _invoiceRepository = new(() => new InvoiceRepository(repositoryContext));
    private readonly Lazy<IUserRepository> _userRepository = new(() => new UserRepository(repositoryContext));

    public ICompanyRepository CompanyRepository => _companyRepository.Value;
    public IInvoiceItemRepository InvoiceItemRepository => _invoiceItemRepository.Value;
    public IInvoiceRepository InvoiceRepository => _invoiceRepository.Value;

    public IUserRepository UserRepository => _userRepository.Value;

    public async Task SaveChanges() =>  await repositoryContext.SaveChangesAsync();
}
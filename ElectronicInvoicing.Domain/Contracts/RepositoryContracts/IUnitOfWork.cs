namespace ElectronicInvoicing.Domain.Contracts.RepositoryContracts;

public interface IUnitOfWork
{
    ICompanyRepository CompanyRepository { get; }
    IInvoiceItemRepository InvoiceItemRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IUserRepository UserRepository { get; }

    Task SaveChanges();
}
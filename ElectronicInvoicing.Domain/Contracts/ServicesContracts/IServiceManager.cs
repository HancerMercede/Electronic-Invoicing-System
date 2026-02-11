namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IServiceManager
{
    ICompanyService CompanyService { get; }
    IInvoiceService InvoiceService { get; }
    IInvoiceItemService InvoiceItemService { get; }
    IAuthService AuthService { get; }
    IUserService UserService { get; }
}
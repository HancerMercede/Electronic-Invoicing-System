namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IServiceManager
{
    ICompanyService CompanyService { get; }
    IInvoiceService InvoiceService { get; }
    IInvoiceItemService InvoiceItemService { get; }
}
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IInvoiceService
{
    Task<Invoice?> GetInvoiceByIdAsync(Guid companyId, Guid invoiceId, bool trackChanges);
    
    Task<IEnumerable<Invoice>>GetAllInvoices(Guid companyId, bool trackChanges);
    
    Task<Invoice> CreateInvoiceAsync(Guid companyId, Invoice invoice);
    
    Task DeleteInvoiceAsync(Guid companyId, Invoice invoice);
}
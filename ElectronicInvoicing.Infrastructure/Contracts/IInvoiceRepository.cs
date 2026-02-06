using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Infrastructure.Contracts;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceById(Guid companyId, Guid invoiceId, bool trackChanges);
    Task<IEnumerable<Invoice>>GetAllInvoice(Guid companyId, bool trackChanges);
    
    Task<Invoice> CreateCompanyAsync(Guid companyId, Invoice invoice);
    
    Task DeleteCompanyAsync(Guid companyId, Invoice invoice);
}
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.RepositoryContracts;

public interface IInvoiceRepository
{
    Task<Invoice?> GetInvoiceById(Guid companyId, Guid invoiceId, bool trackChanges);
    Task<IEnumerable<Invoice>>GetAllInvoice(Guid companyId, bool trackChanges);
    
    Task<Invoice> CreateInvoiceAsync(Guid companyId, Invoice invoice);
    
    Task<Invoice> UpdateAsync(Guid companyId, Invoice invoice);
    
    Task DeleteInvoiceAsync(Guid companyId, Invoice invoice);
    
    
}
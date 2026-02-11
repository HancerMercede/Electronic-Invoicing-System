using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.RepositoryContracts;

public interface IInvoiceItemRepository
{
    Task<InvoiceItem?> GetInvoiceItemById(Guid invoiceId, Guid invoiceItemId);
    Task<IEnumerable<InvoiceItem>>GetAllInvoiceItems(Guid invoiceId, bool trackChanges);
    
    Task<InvoiceItem> CreateInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem);
    
    Task<InvoiceItem>  UpdateInvoiceItem(Guid invoiceId, Guid invoiceItemId, InvoiceItem invoiceItem);
    
    Task DeleteInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem);
}
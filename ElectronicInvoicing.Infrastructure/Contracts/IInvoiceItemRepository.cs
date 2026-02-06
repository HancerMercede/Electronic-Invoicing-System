using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Repositories;

namespace ElectronicInvoicing.Infrastructure.Contracts;

public interface IInvoiceItemRepository
{
    Task<InvoiceItem?> GetInvoiceItemById(Guid invoiceId, Guid invoiceItemId);
    Task<IEnumerable<InvoiceItem>>GetAllInvoiceItems(Guid invoiceId, bool trackChanges);
    
    Task<InvoiceItem> CreateInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem);
    
    Task DeleteInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem);
}
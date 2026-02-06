using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Repositories;

namespace ElectronicInvoicing.Infrastructure.Contracts;

public interface IInvoiceItemRepository
{
    Task<InvoiceItem?> GetInvoiceItemById(Guid invoiceItemId);
    Task<IEnumerable<InvoiceItem>>GetAllInvoiceItems(bool trackChanges);
    
    Task<InvoiceItem> CreateInvoiceItem(InvoiceItem invoiceItem);
    
    Task DeleteInvoiceItem(InvoiceItem invoiceItem);
}
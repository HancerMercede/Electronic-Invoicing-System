using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Application.Services;

public class InvoiceItemService(IUnitOfWork unitOfWork): IInvoiceItemService
{
    public async Task<InvoiceItem?> GetInvoiceItemById(Guid invoiceId, Guid invoiceItemId)
    {
       return  await unitOfWork.InvoiceItemRepository.GetInvoiceItemById(invoiceId, invoiceItemId);
    }

    public async Task<IEnumerable<InvoiceItem>> GetAllInvoiceItems(Guid invoiceId, bool trackChanges)
    {
        return await unitOfWork.InvoiceItemRepository.GetAllInvoiceItems(invoiceId, trackChanges);
    }

    public async Task<InvoiceItem> CreateInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem)
    {
        return  await unitOfWork.InvoiceItemRepository.CreateInvoiceItem(invoiceId, invoiceItem);
    }

    public async Task DeleteInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem)
    {
        await unitOfWork.InvoiceItemRepository.DeleteInvoiceItem(invoiceId, invoiceItem);
    }
}
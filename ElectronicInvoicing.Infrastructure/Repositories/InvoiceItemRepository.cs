using ElectronicInvoicing.Domain.Contracts;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public class InvoiceItemRepository(RepositoryContext repositoryContext):BaseRepository<InvoiceItem>(repositoryContext), IInvoiceItemRepository
{
    public async Task<InvoiceItem?> GetInvoiceItemById(Guid invoiceId,Guid invoiceItemId)
    {
        return await FindByCondiction(i=>i.InvoiceId == invoiceId && i.Id == invoiceItemId, false)
            .SingleOrDefaultAsync()!;
    }

    public async Task<IEnumerable<InvoiceItem>> GetAllInvoiceItems(Guid invoiceId, bool trackChanges)
    {
        return await FindByCondiction(i => i.InvoiceId == invoiceId, trackChanges).ToListAsync();
    }

    public async Task<InvoiceItem> CreateInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem)
    {
        invoiceItem.InvoiceId = invoiceId;
        await AddAsync(invoiceItem);
        return invoiceItem;
    }

    public async Task<InvoiceItem> UpdateInvoiceItem(Guid invoiceId, Guid invoiceItemId, InvoiceItem invoiceItem)
    {
        invoiceItem.InvoiceId = invoiceId;
        await Update(invoiceItem);
        return invoiceItem;
    }

    public async Task DeleteInvoiceItem(Guid invoiceId, InvoiceItem invoiceItem)
    {
        var invoiceItemDb = await FindByCondiction(i => i.InvoiceId == invoiceId && i.Id == invoiceItem.Id, false)
            .SingleOrDefaultAsync();
        
        if (invoiceItemDb is not null)
            await Delete(invoiceItemDb);
    }
}
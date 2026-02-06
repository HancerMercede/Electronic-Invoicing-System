using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Context;
using ElectronicInvoicing.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Repositories;

public class InvoiceRepository(RepositoryContext repositoryContext): BaseRepository<Invoice>(repositoryContext), IInvoiceRepository
{
    public async Task<Invoice?> GetInvoiceById(Guid companyId, Guid invoiceId, bool trackChanges)
    {
        return await FindByCondiction(i => i.CompanyId == companyId && i.Id == invoiceId, trackChanges)
            .Include(i => i.Items)
            .SingleOrDefaultAsync();

    }

    public async Task<IEnumerable<Invoice>> GetAllInvoice(Guid companyId, bool trackChanges)
    {
        return await FindByCondiction(i=>i.CompanyId == companyId, trackChanges).ToListAsync();
    }

    public async Task<Invoice> CreateCompanyAsync(Guid companyId, Invoice invoice)
    {
        invoice.CompanyId = companyId;
        await AddAsync(invoice);
        return invoice;
    }

    public async Task DeleteCompanyAsync(Guid companyId, Invoice invoice)
    {
        var dbInvoice = await FindByCondiction(i => i.CompanyId == companyId && i.Id == invoice.Id, false).SingleOrDefaultAsync();
        if (dbInvoice is not null)
            await Delete(dbInvoice);
    }
}
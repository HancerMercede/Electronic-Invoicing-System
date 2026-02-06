using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Application.Services;

public class InvoiceService(IUnitOfWork unitOfWork):IInvoiceService
{
    public async Task<Invoice?> GetInvoiceByIdAsync(Guid companyId, Guid invoiceId, bool trackChanges)
    {
        return await unitOfWork.InvoiceRepository.GetInvoiceById(companyId, invoiceId, trackChanges);
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoices(Guid companyId, bool trackChanges)
    {
        return await unitOfWork.InvoiceRepository.GetAllInvoice(companyId, trackChanges);
    }

    public async Task<Invoice> CreateInvoiceAsync(Guid companyId, Invoice invoice)
    {
        return await unitOfWork.InvoiceRepository.CreateInvoiceAsync(companyId, invoice);
    }

    public async Task DeleteInvoiceAsync(Guid companyId,Invoice invoice)
    {
        await unitOfWork.InvoiceRepository.DeleteInvoiceAsync(companyId, invoice);
    }
}
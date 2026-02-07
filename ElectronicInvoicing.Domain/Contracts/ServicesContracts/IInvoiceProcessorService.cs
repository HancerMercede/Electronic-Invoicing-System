namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IInvoiceProcessorService
{
    Task<bool> ProcessAndSendAsync(Guid companyId, Guid invoiceId);
}
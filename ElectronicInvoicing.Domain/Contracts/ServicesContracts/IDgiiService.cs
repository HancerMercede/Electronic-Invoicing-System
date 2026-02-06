using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IDgiiService
{
    Task<string?> GetAuthTokenAsync(string certificatePath, string password);

  
    Task<DgiiResponse>  SendInvoiceAsync(string signedXml, string token);
}
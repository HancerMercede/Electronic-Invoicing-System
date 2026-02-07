using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IDgiiService
{
    Task<string?> GetAuthTokenAsync(DigitalCertificateModel cert);

  
    Task<DgiiResponse>  SendInvoiceAsync(string signedXml, string token);

    Task<DgiiResponse> GetStatusAsync(string trackId, string token);
}
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Application.Services;

public class InvoiceProcessorService(IXmlService xmlService, ISignatureService signatureService, IDgiiService dgiiService, IUnitOfWork unitOfWork): IInvoiceProcessorService
{

    public async Task<bool> ProcessAndSendAsync(Guid companyId, Guid invoiceId)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetInvoiceById(companyId, invoiceId, false);
        if (invoice is null) return false;

        try 
        {
            var xmlRaw = await xmlService.GenerateInvoiceXmlAsync(invoice);

             var cert = new DigitalCertificateModel
                        {
                            Content = invoice.Company.DigitalCertificate,
                            Password = invoice.Company.CertificatePassword,
                            Rnc = invoice.Company.Rnc
                        };
            
          
            var (signedXml, securityCode) = await signatureService.SignXmlAsync(xmlRaw, cert);
            
            invoice.SecurityCode = securityCode;
            invoice.Status = InvoiceStatus.Signed;
            await unitOfWork.InvoiceRepository.UpdateAsync(companyId, invoice);
            
            var token = await dgiiService.GetAuthTokenAsync(cert);
            
            var response = await dgiiService.SendInvoiceAsync(signedXml, token!);
            
            if (response.Success)
            {
                invoice.DgiiTrackId = response.TrackId;
                invoice.SentAt = DateTime.UtcNow;
                invoice.Status = InvoiceStatus.Sent;
            }
            else
            {
                invoice.Status = InvoiceStatus.Rejected;
                invoice.RejectionReason = response.Message;
            }

            await unitOfWork.InvoiceRepository.UpdateAsync(companyId, invoice);
            return response.Success;
        }
        catch (Exception ex)
        {
            invoice.Status = InvoiceStatus.Rejected;
            invoice.RejectionReason = $"Critical Error: {ex.Message}";
            await unitOfWork.InvoiceRepository.UpdateAsync(companyId, invoice);
            return false;
        }
    }
}
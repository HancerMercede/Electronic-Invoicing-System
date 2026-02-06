using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IXmlService
{
    Task<string> GenerateInvoiceXmlAsync(Invoice invoice);
    
    Task<bool> ValidateXmlAsync(string xmlContent, string xsdPath);
    
    Task<Invoice> ParseXmlToInvoiceAsync(string xmlContent);
}
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.ExternalsModels;
using ElectronicInvoicing.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;

namespace ElectronicInvoicing.Infrastructure.Services;

public class XmlService(IUnitOfWork unitOfWork, ILogger<XmlService> logger):IXmlService
{
    
    public async Task<string> GenerateInvoiceXmlAsync(Invoice invoice)
    {
        var company = await unitOfWork.CompanyRepository.GetCompanyByIdAsync(invoice.CompanyId);
        
        if (company == null) 
            throw new Exception("La compañía asociada a la factura no existe.");
        
        var ecfDocument = MapToDgiiFormat(invoice, company);

       
        var serializer = new XmlSerializer(typeof(EcfXmlModel));

         using var stringWriter = new Utf8StringWriter();
        
        serializer.Serialize(stringWriter, ecfDocument);

        return stringWriter.ToString();
    }

    public async Task<bool> ValidateXmlAsync(string xmlContent, string xsdPath)
    {
        if (string.IsNullOrWhiteSpace(xmlContent)) return false;

        try
        {
           
            var settings = new XmlReaderSettings();
            settings.Schemas.Add("http://dgii.gov.do/sicfe/dgii/ecf", xsdPath);
            settings.ValidationType = ValidationType.Schema;
            
            settings.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
                {
                    logger.LogError(e.Message);
                    throw new XmlSchemaValidationException(e.Message);
                }
            };

   
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            
            while ( await xmlReader.ReadAsync()) { }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<Invoice> ParseXmlToInvoiceAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new ArgumentException("XML content cannot be empty.");

        try
        {
            var ecfDto = await Task.Run(() => 
            {
                var serializer = new XmlSerializer(typeof(EcfXmlModel));
                using var reader = new StringReader(xmlContent);
                return (EcfXmlModel)serializer.Deserialize(reader)!;
            });
            
            var invoice = MapToDomainEntity(ecfDto);

            return invoice;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse DGII XML to Invoice entity.", ex);
        }
    }
    
    private EcfXmlModel MapToDgiiFormat(Invoice invoice, Company company)
    {
        return new EcfXmlModel
        {
            Header = new HeaderData
            {
                Identification = new DocId
                {
                    EcfType = invoice.ECF.Substring(1, 2), 
                    EcfNumber = invoice.ECF,
                    IssueDate = invoice.IssuedAt.ToString("dd-MM-yyyy"),
                    TaxableIndicator = invoice.IndicatorId.ToString()
                },
                Issuer = new IssuerData
                {
                    Rnc = company.Rnc.Replace("-", ""),
                    Name = company.Name,
                    Address = company.Address
                },
                Receiver = new ReceiverData
                {
                    Rnc = invoice.CustomerRnc?.Replace("-", ""),
                    Name = invoice.CustomerName ?? "CONSUMIDOR FINAL"
                },
                Totals = new TotalsData
                {
                    TaxableAmount = Math.Round(invoice.TaxableAmount, 2),
                    TaxAmount = Math.Round(invoice.TaxAmount, 2),
                    TotalAmount = Math.Round(invoice.TotalAmount, 2)
                }
            },
            Items = invoice.Items.Select(item => new InvoiceItemXml
            {
                LineNumber = invoice.Items.ToList().IndexOf(item) + 1,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalLineAmount = Math.Round(item.Quantity * item.UnitPrice, 2)
            }).ToList()
        };
    }

    private Invoice MapToDomainEntity(EcfXmlModel dto)
    {
     
        var invoice = new Invoice
        {
            ECF = dto.Header.Identification.EcfNumber,
            IssuedAt = DateTime.ParseExact(dto.Header.Identification.IssueDate, "dd-MM-yyyy", null),
            CustomerRnc = dto.Header.Receiver.Rnc,
            CustomerName = dto.Header.Receiver.Name,
            TaxableAmount = dto.Header.Totals.TaxableAmount,
            TaxAmount = dto.Header.Totals.TaxAmount,
            TotalAmount = dto.Header.Totals.TotalAmount,


            Items = dto.Items.Select(item => new InvoiceItem
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            }).ToList()
        };
        return invoice;
    }
}
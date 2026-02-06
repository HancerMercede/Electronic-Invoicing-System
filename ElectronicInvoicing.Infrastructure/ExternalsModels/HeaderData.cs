using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class HeaderData
{
    // Agrupamos todo lo que identifica la factura y los involucrados
    [XmlElement("IdDoc")] public DocId Identification { get; set; } = new();
    [XmlElement("Emisor")] public IssuerData Issuer { get; set; } = new();
    [XmlElement("Receptor")] public ReceiverData Receiver { get; set; } = new();
    [XmlElement("Totales")] public TotalsData Totals { get; set; } = new();
}
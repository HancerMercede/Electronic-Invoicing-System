using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class TotalsData
{
    [XmlElement("MontoGravadoTotal")]
    public decimal TaxableAmount { get; set; }

    [XmlElement("ITBISTotal")]
    public decimal TaxAmount { get; set; }

    [XmlElement("MontoTotal")]
    public decimal TotalAmount { get; set; }
}
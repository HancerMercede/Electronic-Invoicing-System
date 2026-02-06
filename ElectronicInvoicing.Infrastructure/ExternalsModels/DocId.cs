using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class DocId
{
    [XmlElement("TipoeCF")]
    public string EcfType { get; set; } = string.Empty;

    [XmlElement("ENFC")]
    public string EcfNumber { get; set; } = string.Empty;

    [XmlElement("FechaEmision")]
    public string IssueDate { get; set; } = string.Empty;

    [XmlElement("IndicadorMontoGravado")]
    public string TaxableIndicator { get; set; } = string.Empty;
}
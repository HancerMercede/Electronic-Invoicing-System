using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class IssuerData
{
    [XmlElement("RNCemisor")]
    public string Rnc { get; set; } = string.Empty;

    [XmlElement("RazonSocialEmisor")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("DireccionEmisor")]
    public string? Address { get; set; } = string.Empty;
}
using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class ReceiverData
{
    [XmlElement("RNCReceptor")]
    public string? Rnc { get; set; } = string.Empty;

    [XmlElement("RazonSocialReceptor")]
    public string? Name { get; set; } = string.Empty;
}
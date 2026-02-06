using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

[XmlRoot("TokenResponse")] // Nombre del nodo según el XSD de la DGII
public class DgiiTokenResponse
{
    [XmlElement("token")]
    public string Token { get; set; }

    [XmlElement("expiracion")]
    public string Expiration { get; set; }
}
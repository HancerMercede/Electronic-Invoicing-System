using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

[XmlRoot("recepcion")]
public class DgiiReceptionResponse
{
    [XmlElement("trackId")]
    public string TrackId { get; set; }

    [XmlElement("codigo")]
    public string Code { get; set; }

    [XmlElement("mensaje")]
    public string Message { get; set; }

    [XmlArray("errores")]
    [XmlArrayItem("error")]
    public List<DgiiError> Errors { get; set; }

    public bool Success => !string.IsNullOrEmpty(TrackId);
}
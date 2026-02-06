using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class EcfXmlModel
{
    // Esta es la "caja" de encabezado que pide la DGII
    [XmlElement("Encabezado")]
    public HeaderData Header { get; set; } = new();

    [XmlArray("DetalleItems")]
    [XmlArrayItem("Item")]
    public List<InvoiceItemXml> Items { get; set; } = new();
}


using System.Xml.Serialization;

namespace ElectronicInvoicing.Infrastructure.ExternalsModels;

public class InvoiceItemXml
{
    [XmlElement("NumeroLinea")]
    public int LineNumber { get; set; }

    // '1' para Bienes, '2' para Servicios
    [XmlElement("IndicadorBienOServicio")]
    public string ItemTypeIndicator { get; set; } = "1"; 

    [XmlElement("NombreItem")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("CantidadItem")]
    public decimal Quantity { get; set; }

    [XmlElement("PrecioUnitarioItem")]
    public decimal UnitPrice { get; set; }

    // MontoItem = Cantidad * Precio (debe ir redondeado a 2 decimales)
    [XmlElement("MontoItem")]
    public decimal TotalLineAmount { get; set; }
}
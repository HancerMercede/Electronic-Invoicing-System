using System.ComponentModel.DataAnnotations;
using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Domain.Entities;

public class Invoice
{
    [Key]
    public Guid Id { get; set; }

    // --- Control DGII (e-CF) ---
    [Required]
    [StringLength(11)]
    public string ECF { get; set; } = string.Empty; // Ejemplo: E3100000001
    
    [Required]
    public int IndicatorId { get; set; } // 31 = Consumo, 32 = Crédito Fiscal, etc.

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpirationDate { get; set; } // Para NCFs con vencimiento

    // --- Información del Emisor (Tu Empresa) ---
    [Required]
    public string IssuerRnc { get; set; } = string.Empty;
    public string IssuerCompanyName { get; set; } = string.Empty;

    // --- Información del Cliente (Receptor) ---
    [StringLength(11)]
    public string? CustomerRnc { get; set; } // Opcional para consumo menor a RD$250,000
    public string? CustomerName { get; set; }

    // --- Totales Financieros ---
    public decimal TotalAmount { get; set; }      // Monto Total (G)
    public decimal TaxableAmount { get; set; }    // Monto Gravado (EI)
    public decimal TaxAmount { get; set; }         // ITBIS (I1)
    public decimal DiscountAmount { get; set; }
    public decimal ExemptAmount { get; set; }     // Monto Exento (E)

    // --- Estado de Facturación Electrónica ---
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    // El TrackId es vital: es el comprobante de que la DGII recibió el archivo
    public string? DgiiTrackId { get; set; } 
    
    // SecurityCode es el "Hash" o firma que genera el proceso de firmado XML
    public string? SecurityCode { get; set; }
    
    public string? DgiiResponseCode { get; set; } // Ej: "0" para Aceptado
    public string? RejectionReason { get; set; }

    // --- Relaciones ---
    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}

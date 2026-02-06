using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.Entities;

public class InvoiceItem
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid InvoiceId { get; set; }
    
    [Required]
    public string ProductCode { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;

    public double Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal Discount { get; set; }

    // ITBIS por cada línea
    public decimal TaxAmount { get; set; }
    
    // Subtotal de la línea: (Quantity * UnitPrice) - Discount + TaxAmount
    public decimal LineTotal { get; set; }

    // Navegación
    public virtual Invoice Invoice { get; set; } = null!;
}
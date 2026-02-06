using System.ComponentModel.DataAnnotations;
using ElectronicInvoicing.Domain.Contracts;

namespace ElectronicInvoicing.Domain.Entities;

public class InvoiceItem:ITenantEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid InvoiceId { get; set; }
    
    [Required]
    public string ProductCode { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal Discount { get; set; }

    // ITBIS por cada línea
    public decimal TaxAmount { get; set; }
    
    // Subtotal de la línea: (Quantity * UnitPrice) - Discount + TaxAmount
    public decimal LineTotal { get; set; }

    // Navegación
    public virtual Invoice Invoice { get; set; } = null!;
    public Guid CompanyId { get; set; }
}
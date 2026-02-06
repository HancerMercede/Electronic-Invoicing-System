using System.ComponentModel.DataAnnotations;
using ElectronicInvoicing.Domain.Contracts;
using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Domain.Entities;

public class Invoice: ITenantEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(11)]
    public string ECF { get; set; } = string.Empty; 
    
    [Required]
    public int IndicatorId { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpirationDate { get; set; } 
    
    [Required]
    public string IssuerRnc { get; set; } = string.Empty;
    public string IssuerCompanyName { get; set; } = string.Empty;

   
    [StringLength(11)]
    public string? CustomerRnc { get; set; }
    public string? CustomerName { get; set; }


    public decimal TotalAmount { get; set; }  
    public decimal TaxableAmount { get; set; }   
    public decimal TaxAmount { get; set; }        
    public decimal DiscountAmount { get; set; }
    public decimal ExemptAmount { get; set; }     


    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    

    public string? DgiiTrackId { get; set; } 
    
 
    public string? SecurityCode { get; set; }
    
    public string? DgiiResponseCode { get; set; } 
    public string? RejectionReason { get; set; }


    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public Guid CompanyId { get; set; }
}

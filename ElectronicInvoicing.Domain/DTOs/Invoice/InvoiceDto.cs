using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Domain.DTOs.Invoice;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string ECF { get; set; } = string.Empty;
    public int IndicatorId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string IssuerRnc { get; set; } = string.Empty;
    public string IssuerCompanyName { get; set; } = string.Empty;
    public string? CustomerRnc { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ExemptAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? DgiiTrackId { get; set; }
    public string? SecurityCode { get; set; }
    public string? DgiiResponseCode { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public string? QrContent { get; set; }
    public Guid CompanyId { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

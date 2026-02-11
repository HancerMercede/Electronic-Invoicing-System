using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.DTOs.Invoice;

public class UpdateInvoiceDto
{
    public DateTime? ExpirationDate { get; set; }

    [StringLength(11, MinimumLength = 9, ErrorMessage = "Customer RNC must be between 9 and 11 characters")]
    public string? CustomerRnc { get; set; }

    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public string? CustomerName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Exempt amount cannot be negative")]
    public decimal ExemptAmount { get; set; }

    public List<CreateInvoiceItemDto>? Items { get; set; }
}

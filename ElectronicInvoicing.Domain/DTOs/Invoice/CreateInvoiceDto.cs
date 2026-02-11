using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.DTOs.Invoice;

public class CreateInvoiceDto
{
    [Required(ErrorMessage = "ECF is required")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "ECF must be exactly 11 characters")]
    public string ECF { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indicator ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Indicator ID must be greater than 0")]
    public int IndicatorId { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [StringLength(11, MinimumLength = 9, ErrorMessage = "Customer RNC must be between 9 and 11 characters")]
    public string? CustomerRnc { get; set; }

    [StringLength(200, ErrorMessage = "Customer name cannot exceed 200 characters")]
    public string? CustomerName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Exempt amount cannot be negative")]
    public decimal ExemptAmount { get; set; } = 0;

    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "Invoice must have at least one item")]
    public List<CreateInvoiceItemDto> Items { get; set; } = new();
}

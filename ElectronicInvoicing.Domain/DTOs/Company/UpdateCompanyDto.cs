using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.DTOs.Company;

public class UpdateCompanyDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Commercial name cannot exceed 200 characters")]
    public string? CommercialName { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }

    public bool IsElectronicIssuer { get; set; }

    public string? DigitalCertificateBase64 { get; set; }

    [StringLength(100, ErrorMessage = "Certificate password cannot exceed 100 characters")]
    public string? CertificatePassword { get; set; }

    public DateTime? CertificateExpiration { get; set; }

    [StringLength(100, ErrorMessage = "API Client ID cannot exceed 100 characters")]
    public string? ApiClientId { get; set; }

    [StringLength(100, ErrorMessage = "API Client Secret cannot exceed 100 characters")]
    public string? ApiClientSecret { get; set; }

    public bool IsActive { get; set; }
}

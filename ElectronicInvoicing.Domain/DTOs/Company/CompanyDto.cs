namespace ElectronicInvoicing.Domain.DTOs.Company;

public class 
    CompanyDto
{
    public Guid Id { get; set; }
    public string Rnc { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CommercialName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsElectronicIssuer { get; set; }
    public DateTime? CertificateExpiration { get; set; }
    public bool HasDigitalCertificate { get; set; }
    public string? ApiClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

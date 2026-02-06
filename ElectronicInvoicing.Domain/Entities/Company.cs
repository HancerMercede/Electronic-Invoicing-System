using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.Entities;

public class Company
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Rnc { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty; 

    public string? CommercialName { get; set; }

    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }

    
    [Required]
    public bool IsElectronicIssuer { get; set; } = true;


    /// <summary>
    /// El certificado .p12 almacenado en formato binario.
    /// </summary>
    public byte[]? DigitalCertificate { get; set; }

    /// <summary>
    /// Contraseña del certificado (Debe manejarse con precaución/encriptación).
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// Fecha de vencimiento del certificado para alertas preventivas.
    /// </summary>
    public DateTime? CertificateExpiration { get; set; }
    

    public string? ApiClientId { get; set; }
    public string? ApiClientSecret { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
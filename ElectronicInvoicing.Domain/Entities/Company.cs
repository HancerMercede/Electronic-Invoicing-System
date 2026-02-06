using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.Entities;

public class Company
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Rnc { get; set; } = string.Empty; // RNC de 9 o 11 dígitos

    [Required]
    public string Name { get; set; } = string.Empty; // Razón Social

    public string? CommercialName { get; set; } // Nombre Comercial

    public string? Address { get; set; }
    
    public string? PhoneNumber { get; set; }

    // --- Configuración Fiscal ---
    
    [Required]
    public bool IsElectronicIssuer { get; set; } = true;

    // --- Seguridad y Firma Digital ---
    
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

    // --- Credenciales API DGII ---
    
    // ClientId y Secret si utilizas un middleware o si la DGII requiere auth específica
    public string? ApiClientId { get; set; }
    public string? ApiClientSecret { get; set; }

    // --- Auditoría ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // --- Relaciones ---
    // Una empresa puede tener muchas facturas emitidas
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
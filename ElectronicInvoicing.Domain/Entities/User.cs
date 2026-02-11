using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ElectronicInvoicing.Domain.Contracts;
using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Domain.Entities;

public class User : ITenantEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(200)]
    public string? FullName { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; } = null!;
}

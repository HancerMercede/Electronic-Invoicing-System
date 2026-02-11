using ElectronicInvoicing.Domain.Enums;

namespace ElectronicInvoicing.Domain.DTOs.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime ExpiresAt { get; set; }
}

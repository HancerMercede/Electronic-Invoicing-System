using System.ComponentModel.DataAnnotations;

namespace ElectronicInvoicing.Domain.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Username or email is required")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}

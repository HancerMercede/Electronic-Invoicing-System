using ElectronicInvoicing.Domain.DTOs.Auth;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    string GenerateJwtToken(Guid userId, string username, string email, string role, Guid companyId);
}

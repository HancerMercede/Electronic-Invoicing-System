using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.DTOs.Auth;
using ElectronicInvoicing.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ElectronicInvoicing.Application.Services;

public class AuthService(IUnitOfWork unitOfWork, IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by username or email
        var user = await unitOfWork.UserRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password.");

        // Verify password
        if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        // Check if user is active
        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await unitOfWork.UserRepository.UpdateAsync(user);
        await unitOfWork.SaveChanges();

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.Role.ToString(), user.CompanyId);

        // Get token expiration from config
        var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            CompanyId = user.CompanyId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if username already exists
        var existingUser = await unitOfWork.UserRepository.GetByUsernameAsync(registerDto.Username);
        if (existingUser != null)
            throw new InvalidOperationException("Username already exists.");

        // Check if email already exists
        var existingEmail = await unitOfWork.UserRepository.GetByEmailAsync(registerDto.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("Email already exists.");

        // Verify company exists
        var company = await unitOfWork.CompanyRepository.GetCompanyByIdAsync(registerDto.CompanyId);
        if (company == null)
            throw new InvalidOperationException("Company not found.");

        // Hash password
        var passwordHash = HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            FullName = registerDto.FullName,
            CompanyId = registerDto.CompanyId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await unitOfWork.UserRepository.CreateAsync(user);
        await unitOfWork.SaveChanges();

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.Username, user.Email, user.Role.ToString(), user.CompanyId);

        // Get token expiration from config
        var expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            CompanyId = user.CompanyId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public string GenerateJwtToken(Guid userId, string username, string email, string role, Guid companyId)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = jwtSettings["Issuer"] ?? "ElectronicInvoicingAPI";
        var audience = jwtSettings["Audience"] ?? "ElectronicInvoicingClient";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("CompanyId", companyId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

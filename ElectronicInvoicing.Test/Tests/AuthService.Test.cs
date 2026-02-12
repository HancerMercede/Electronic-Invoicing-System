using ElectronicInvoicing.Application.Services;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.DTOs.Auth;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ElectronicInvoicing.Test.Tests;

public class AuthService_Test
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthService _sut;

    public AuthService_Test()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup UnitOfWork
        _mockUnitOfWork.Setup(x => x.UserRepository).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.SaveChanges()).Returns(Task.CompletedTask);

        // Setup JWT Configuration
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(x => x["SecretKey"]).Returns("TestSecretKeyForJWTThatIsAtLeast32CharactersLong!");
        mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
        mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
        mockJwtSection.Setup(x => x["ExpirationMinutes"]).Returns("60");

        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(mockJwtSection.Object);
        _mockConfiguration.Setup(x => x["Jwt:ExpirationMinutes"]).Returns("60");

        _sut = new AuthService(_mockUnitOfWork.Object, _mockConfiguration.Object);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            FullName = "Test User",
            CompanyId = Guid.NewGuid()
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(registerDto.Username))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        var mockCompanyRepo = new Mock<ICompanyRepository>();
        mockCompanyRepo
            .Setup(x => x.GetCompanyByIdAsync(registerDto.CompanyId))
            .ReturnsAsync(new Company { Id = registerDto.CompanyId, Name = "Test Company" });

        _mockUnitOfWork.Setup(x => x.CompanyRepository).Returns(mockCompanyRepo.Object);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);

        // Act
        var result = await _sut.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be(registerDto.Username);
        result.Email.Should().Be(registerDto.Email);
        result.CompanyId.Should().Be(registerDto.CompanyId);

        _mockUserRepository.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Username == registerDto.Username &&
            u.Email == registerDto.Email &&
            u.FullName == registerDto.FullName &&
            u.CompanyId == registerDto.CompanyId &&
            u.IsActive == true
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "SecurePass123!",
            CompanyId = Guid.NewGuid()
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(registerDto.Username))
            .ReturnsAsync(new User { Username = "existinguser" });

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Username already exists.");

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "SecurePass123!",
            CompanyId = Guid.NewGuid()
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(registerDto.Username))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_WithNonExistentCompany_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            CompanyId = Guid.NewGuid()
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(registerDto.Username))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        var mockCompanyRepo = new Mock<ICompanyRepository>();
        mockCompanyRepo
            .Setup(x => x.GetCompanyByIdAsync(registerDto.CompanyId))
            .ReturnsAsync((Company?)null);

        _mockUnitOfWork.Setup(x => x.CompanyRepository).Returns(mockCompanyRepo.Object);

        // Act
        Func<Task> act = async () => await _sut.RegisterAsync(registerDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Company not found.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "PlainTextPassword123!",
            CompanyId = Guid.NewGuid()
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var mockCompanyRepo = new Mock<ICompanyRepository>();
        mockCompanyRepo.Setup(x => x.GetCompanyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Company { Id = registerDto.CompanyId });
        _mockUnitOfWork.Setup(x => x.CompanyRepository).Returns(mockCompanyRepo.Object);

        User? capturedUser = null;
        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User user) => user);

        // Act
        await _sut.RegisterAsync(registerDto);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(registerDto.Password);
        capturedUser.PasswordHash.Should().StartWith("$2"); // BCrypt hash starts with $2
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var plainPassword = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FullName = "Test User",
            Role = UserRole.User,
            CompanyId = Guid.NewGuid(),
            IsActive = true
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = plainPassword
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.CompanyId.Should().Be(user.CompanyId);

        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.LastLoginAt != null
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "nonexistent",
            Password = "SomePassword123!"
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");

        var user = new User
        {
            Username = "testuser",
            PasswordHash = hashedPassword,
            IsActive = true
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "WrongPassword123!"
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var plainPassword = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        var user = new User
        {
            Username = "testuser",
            PasswordHash = hashedPassword,
            IsActive = false // User is inactive
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = plainPassword
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(loginDto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User account is inactive.");
    }

    [Fact]
    public async Task LoginAsync_ShouldUpdateLastLoginTimestamp()
    {
        // Arrange
        var plainPassword = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            Role = UserRole.User,
            CompanyId = Guid.NewGuid(),
            IsActive = true,
            LastLoginAt = null
        };

        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = plainPassword
        };

        _mockUserRepository
            .Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        User? updatedUser = null;
        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LoginAsync(loginDto);

        // Assert
        updatedUser.Should().NotBeNull();
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region GenerateJwtToken Tests

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "Admin";
        var companyId = Guid.NewGuid();

        // Act
        var token = _sut.GenerateJwtToken(userId, username, email, role, companyId);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts: header.payload.signature
    }

    #endregion
}

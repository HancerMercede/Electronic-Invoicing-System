using ElectronicInvoicing.Application.Services;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ElectronicInvoicing.Test.Tests;

public class CompanyService_Test
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<ICompanyRepository> _mockCompanyRepository;
    private readonly CompanyService _sut; // System Under Test

    public CompanyService_Test()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();

        // Setup UnitOfWork to return mocked repository
        _mockUnitOfWork.Setup(x => x.CompanyRepository).Returns(_mockCompanyRepository.Object);
        _mockUnitOfWork.Setup(x => x.SaveChanges()).Returns(Task.CompletedTask);

        _sut = new CompanyService(_mockUnitOfWork.Object, _mockEncryptionService.Object);
    }

    #region GetCompanyByIdAsync Tests

    [Fact]
    public async Task GetCompanyByIdAsync_WhenCompanyExists_ShouldReturnDecryptedCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var encryptedCompany = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company",
            CertificatePassword = "encrypted_password",
            ApiClientSecret = "encrypted_secret",
            DigitalCertificate = new byte[] { 1, 2, 3 }
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(encryptedCompany);

        _mockEncryptionService
            .Setup(x => x.Decrypt("encrypted_password"))
            .Returns("decrypted_password");

        _mockEncryptionService
            .Setup(x => x.Decrypt("encrypted_secret"))
            .Returns("decrypted_secret");

        _mockEncryptionService
            .Setup(x => x.DecryptBytes(It.IsAny<byte[]>()))
            .Returns(new byte[] { 4, 5, 6 });

        // Act
        var result = await _sut.GetCompanyByIdAsync(companyId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(companyId);
        result.Rnc.Should().Be("131793916");

        // Verify decryption was called
        _mockEncryptionService.Verify(x => x.Decrypt("encrypted_password"), Times.Once);
        _mockEncryptionService.Verify(x => x.Decrypt("encrypted_secret"), Times.Once);
        _mockEncryptionService.Verify(x => x.DecryptBytes(It.IsAny<byte[]>()), Times.Once);

        _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task GetCompanyByIdAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _sut.GetCompanyByIdAsync(companyId);

        // Assert
        result.Should().BeNull();
        _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId), Times.Once);

        // Verify decryption was NOT called
        _mockEncryptionService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetCompanyByIdAsync_WhenSensitiveDataIsNull_ShouldNotCallDecryption()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company",
            CertificatePassword = null,
            ApiClientSecret = null,
            DigitalCertificate = null
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _sut.GetCompanyByIdAsync(companyId);

        // Assert
        result.Should().NotBeNull();

        // Verify decryption was NOT called for null values
        _mockEncryptionService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Never);
        _mockEncryptionService.Verify(x => x.DecryptBytes(It.IsAny<byte[]>()), Times.Never);
    }

    #endregion

    #region GetAllCompanies Tests

    [Fact]
    public async Task GetAllCompanies_ShouldReturnDecryptedCompanies()
    {
        // Arrange
        var companies = new List<Company>
        {
            new Company
            {
                Id = Guid.NewGuid(),
                Rnc = "131793916",
                Name = "Company 1",
                CertificatePassword = "encrypted_pass1"
            },
            new Company
            {
                Id = Guid.NewGuid(),
                Rnc = "131793917",
                Name = "Company 2",
                CertificatePassword = "encrypted_pass2"
            }
        };

        _mockCompanyRepository
            .Setup(x => x.GetAllCompanies(It.IsAny<bool>()))
            .ReturnsAsync(companies);

        _mockEncryptionService
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns((string input) => $"decrypted_{input}");

        // Act
        var result = await _sut.GetAllCompanies(false);

        // Assert
        result.Should().HaveCount(2);

        // Verify decryption was called for each company
        _mockEncryptionService.Verify(x => x.Decrypt(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllCompanies_WhenNoCompanies_ShouldReturnEmptyList()
    {
        // Arrange
        _mockCompanyRepository
            .Setup(x => x.GetAllCompanies(It.IsAny<bool>()))
            .ReturnsAsync(new List<Company>());

        // Act
        var result = await _sut.GetAllCompanies(false);

        // Assert
        result.Should().BeEmpty();
        _mockCompanyRepository.Verify(x => x.GetAllCompanies(false), Times.Once);
    }

    #endregion

    #region CreateCompanyAsync Tests

    [Fact]
    public async Task CreateCompanyAsync_ShouldEncryptSensitiveDataBeforeSaving()
    {
        // Arrange
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Rnc = "131793916",
            Name = "New Company",
            CertificatePassword = "plain_password",
            ApiClientSecret = "plain_secret",
            DigitalCertificate = new byte[] { 1, 2, 3 }
        };

        _mockEncryptionService
            .Setup(x => x.Encrypt("plain_password"))
            .Returns("encrypted_password");

        _mockEncryptionService
            .Setup(x => x.Encrypt("plain_secret"))
            .Returns("encrypted_secret");

        _mockEncryptionService
            .Setup(x => x.EncryptBytes(It.IsAny<byte[]>()))
            .Returns(new byte[] { 9, 9, 9 });

        // Setup decryption mocks (needed when returning decrypted company)
        _mockEncryptionService.Setup(x => x.Decrypt("encrypted_password"))
            .Returns("plain_password");
        
        _mockEncryptionService.Setup(x => x.Decrypt("encrypted_secret"))
            .Returns("plain_secret");
        
        _mockEncryptionService.Setup(x => x.DecryptBytes(It.IsAny<byte[]>()))
            .Returns(new byte[] { 1,2,3 });
        
        _mockCompanyRepository
            .Setup(x => x.CreateCompanyAsync(It.IsAny<Company>()))
            .ReturnsAsync((Company c) => c);

        // Act
        var result = await _sut.CreateCompanyAsync(company);

        // Assert
        result.Should().NotBeNull();

        // Verify encryption was called before saving
        _mockEncryptionService.Verify(x => x.Encrypt("plain_password"), Times.Once);
        _mockEncryptionService.Verify(x => x.Encrypt("plain_secret"), Times.Once);
        _mockEncryptionService.Verify(x => x.EncryptBytes(It.IsAny<byte[]>()), Times.Once); // Encrypt

        // Verify decryption was called before returning
        _mockEncryptionService.Verify(x => x.Decrypt("encrypted_password"), Times.Once);
        _mockEncryptionService.Verify(x => x.Decrypt("encrypted_secret"), Times.Once);
        _mockEncryptionService.Verify(x => x.DecryptBytes(It.IsAny<byte[]>()), Times.Once); // Decrypt

        _mockCompanyRepository.Verify(x => x.CreateCompanyAsync(It.IsAny<Company>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task CreateCompanyAsync_ShouldReturnDecryptedCompany()
    {
        // Arrange
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Rnc = "131793916",
            Name = "New Company",
            CertificatePassword = "plain_password"
        };

        _mockEncryptionService
            .Setup(x => x.Encrypt(It.IsAny<string>()))
            .Returns("encrypted_data");

        _mockEncryptionService
            .Setup(x => x.Decrypt("encrypted_data"))
            .Returns("decrypted_data");

        _mockCompanyRepository
            .Setup(x => x.CreateCompanyAsync(It.IsAny<Company>()))
            .ReturnsAsync((Company c) => c);

        // Act
        var result = await _sut.CreateCompanyAsync(company);

        // Assert
        result.Should().NotBeNull();
        result.CertificatePassword.Should().Be("decrypted_data");
    }

    #endregion

    #region UpdateCompanyAsync Tests

    [Fact]
    public async Task UpdateCompanyAsync_ShouldEncryptSensitiveDataBeforeUpdating()
    {
        // Arrange
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Rnc = "131793916",
            Name = "Updated Company",
            CertificatePassword = "new_password",
            ApiClientSecret = "new_secret"
        };

        _mockEncryptionService
            .Setup(x => x.Encrypt("new_password"))
            .Returns("encrypted_new_password");

        _mockEncryptionService
            .Setup(x => x.Encrypt("new_secret"))
            .Returns("encrypted_new_secret");

        _mockEncryptionService
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns((string input) => input.Replace("encrypted_", "decrypted_"));

        _mockCompanyRepository
            .Setup(x => x.UpdateCompanyAsync(It.IsAny<Company>()))
            .ReturnsAsync((Company c) => c);

        // Act
        var result = await _sut.UpdateCompanyAsync(company);

        // Assert
        result.Should().NotBeNull();

        // Verify encryption was called
        _mockEncryptionService.Verify(x => x.Encrypt("new_password"), Times.Once);
        _mockEncryptionService.Verify(x => x.Encrypt("new_secret"), Times.Once);

        _mockCompanyRepository.Verify(x => x.UpdateCompanyAsync(It.IsAny<Company>()), Times.Once);
    }

    #endregion

    #region DeleteCompanyAsync Tests

    [Fact]
    public async Task DeleteCompanyAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Rnc = "131793916",
            Name = "Company to Delete"
        };

        _mockCompanyRepository
            .Setup(x => x.DeleteCompanyAsync(company))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteCompanyAsync(company);

        // Assert
        _mockCompanyRepository.Verify(x => x.DeleteCompanyAsync(company), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChanges(), Times.Once);
    }

    #endregion
}

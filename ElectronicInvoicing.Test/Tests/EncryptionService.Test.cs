using ElectronicInvoicing.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ElectronicInvoicing.Test.Tests;

public class EncryptionService_Test
{
    private readonly EncryptionService _sut;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public EncryptionService_Test()
    {
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup encryption key
        _mockConfiguration
            .Setup(x => x["Encryption:Key"])
            .Returns("TestEncryptionKeyForUnitTests2026SecureKey!");

        _sut = new EncryptionService(_mockConfiguration.Object);
    }

    #region Encrypt/Decrypt String Tests

    [Fact]
    public void Encrypt_WithValidPlainText_ShouldReturnEncryptedString()
    {
        // Arrange
        var plainText = "MySecretPassword123!";

        // Act
        var encryptedText = _sut.Encrypt(plainText);

        // Assert
        encryptedText.Should().NotBeNullOrEmpty();
        encryptedText.Should().NotBe(plainText);
        encryptedText.Length.Should().BeGreaterThan(plainText.Length); // Encrypted + IV is longer
    }

    [Fact]
    public void Decrypt_WithValidEncryptedText_ShouldReturnOriginalPlainText()
    {
        // Arrange
        var originalText = "MySecretPassword123!";
        var encryptedText = _sut.Encrypt(originalText);

        // Act
        var decryptedText = _sut.Decrypt(encryptedText);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void Encrypt_SameTextTwice_ShouldProduceDifferentResults()
    {
        // Arrange
        var plainText = "MySecretPassword123!";

        // Act
        var encrypted1 = _sut.Encrypt(plainText);
        var encrypted2 = _sut.Encrypt(plainText);

        // Assert
        encrypted1.Should().NotBe(encrypted2); // Different IV each time
    }

    [Fact]
    public void Decrypt_BothEncryptedVersions_ShouldReturnSamePlainText()
    {
        // Arrange
        var plainText = "MySecretPassword123!";
        var encrypted1 = _sut.Encrypt(plainText);
        var encrypted2 = _sut.Encrypt(plainText);

        // Act
        var decrypted1 = _sut.Decrypt(encrypted1);
        var decrypted2 = _sut.Decrypt(encrypted2);

        // Assert
        decrypted1.Should().Be(plainText);
        decrypted2.Should().Be(plainText);
        decrypted1.Should().Be(decrypted2);
    }

    [Fact]
    public void Encrypt_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var plainText = string.Empty;

        // Act
        var encryptedText = _sut.Encrypt(plainText);

        // Assert
        encryptedText.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_WithNullString_ShouldReturnNull()
    {
        // Arrange
        string? plainText = null;

        // Act
        var encryptedText = _sut.Encrypt(plainText!);

        // Assert
        encryptedText.Should().BeNull();
    }

    [Fact]
    public void Decrypt_WithNullString_ShouldReturnNull()
    {
        // Arrange
        string? cipherText = null;

        // Act
        var decryptedText = _sut.Decrypt(cipherText!);

        // Assert
        decryptedText.Should().BeNull();
    }

    [Theory]
    [InlineData("SimplePassword")]
    [InlineData("Complex!P@ssw0rd#2024")]
    [InlineData("12345")]
    [InlineData("Unicode_Test_中文_العربية_🔒")]
    [InlineData("Very long password with many characters to test encryption of longer strings that exceed normal password lengths")]
    public void EncryptDecrypt_WithVariousStrings_ShouldMaintainDataIntegrity(string plainText)
    {
        // Act
        var encrypted = _sut.Encrypt(plainText);
        var decrypted = _sut.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    #endregion

    #region EncryptBytes/DecryptBytes Tests

    [Fact]
    public void EncryptBytes_WithValidData_ShouldReturnEncryptedBytes()
    {
        // Arrange
        var plainBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        // Act
        var encryptedBytes = _sut.EncryptBytes(plainBytes);

        // Assert
        encryptedBytes.Should().NotBeNull();
        encryptedBytes.Length.Should().BeGreaterThan(plainBytes.Length); // Includes IV
        encryptedBytes.Should().NotBeEquivalentTo(plainBytes);
    }

    [Fact]
    public void DecryptBytes_WithValidEncryptedData_ShouldReturnOriginalBytes()
    {
        // Arrange
        var originalBytes = new byte[] { 10, 20, 30, 40, 50 };
        var encryptedBytes = _sut.EncryptBytes(originalBytes);

        // Act
        var decryptedBytes = _sut.DecryptBytes(encryptedBytes);

        // Assert
        decryptedBytes.Should().BeEquivalentTo(originalBytes);
    }

    [Fact]
    public void EncryptBytes_SameBytesArrayTwice_ShouldProduceDifferentResults()
    {
        // Arrange
        var plainBytes = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var encrypted1 = _sut.EncryptBytes(plainBytes);
        var encrypted2 = _sut.EncryptBytes(plainBytes);

        // Assert
        encrypted1.Should().NotBeEquivalentTo(encrypted2); // Different IV
    }

    [Fact]
    public void DecryptBytes_BothEncryptedVersions_ShouldReturnSameOriginalBytes()
    {
        // Arrange
        var plainBytes = new byte[] { 100, 200, 255, 0, 128 };
        var encrypted1 = _sut.EncryptBytes(plainBytes);
        var encrypted2 = _sut.EncryptBytes(plainBytes);

        // Act
        var decrypted1 = _sut.DecryptBytes(encrypted1);
        var decrypted2 = _sut.DecryptBytes(encrypted2);

        // Assert
        decrypted1.Should().BeEquivalentTo(plainBytes);
        decrypted2.Should().BeEquivalentTo(plainBytes);
        decrypted1.Should().BeEquivalentTo(decrypted2);
    }

    [Fact]
    public void EncryptBytes_WithEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var emptyBytes = Array.Empty<byte>();

        // Act
        var encryptedBytes = _sut.EncryptBytes(emptyBytes);

        // Assert
        encryptedBytes.Should().BeEmpty();
    }

    [Fact]
    public void EncryptBytes_WithNullArray_ShouldReturnNull()
    {
        // Arrange
        byte[]? nullBytes = null;

        // Act
        var encryptedBytes = _sut.EncryptBytes(nullBytes!);

        // Assert
        encryptedBytes.Should().BeNull();
    }

    [Fact]
    public void EncryptDecryptBytes_WithLargeByteArray_ShouldMaintainDataIntegrity()
    {
        // Arrange - Simulate a .p12 certificate (large binary data)
        var largeBytes = new byte[10000];
        new Random().NextBytes(largeBytes);

        // Act
        var encrypted = _sut.EncryptBytes(largeBytes);
        var decrypted = _sut.DecryptBytes(encrypted);

        // Assert
        decrypted.Should().BeEquivalentTo(largeBytes);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Constructor_WithoutEncryptionKey_ShouldThrowException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(x => x["Encryption:Key"]).Returns((string?)null);

        // Act
        Action act = () => new EncryptionService(mockConfig.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Encryption key is not configured*");
    }

    #endregion
}

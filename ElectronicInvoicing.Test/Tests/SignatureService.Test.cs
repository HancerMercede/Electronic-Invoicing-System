using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Infrastructure.Services;
using FluentAssertions;

namespace ElectronicInvoicing.Test.Tests;

public class SignatureService_Test
{
    private readonly SignatureService _sut;

    public SignatureService_Test()
    {
        _sut = new SignatureService();
    }

    #region SignXmlAsync Tests

    [Fact]
    public async Task SignXmlAsync_WithValidXmlAndCertificate_ShouldReturnSignedXml()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <TipoEcf>31</TipoEcf>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        // Create a test certificate
        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result = await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        result.SignedXml.Should().NotBeNullOrEmpty();
        result.SignedXml.Should().Contain("<?xml");
        result.SignedXml.Should().Contain("<ECF>");
        result.SignedXml.Should().Contain("<Signature"); // XML Signature element
        result.SecurityCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SignXmlAsync_ShouldIncludeOriginalXmlContent()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result = await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        result.SignedXml.Should().Contain("E310000000001"); // Original content preserved
        result.SignedXml.Should().Contain("<Encabezado>");
    }

    [Fact]
    public async Task SignXmlAsync_ShouldReturnNonEmptySecurityCode()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result = await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        result.SecurityCode.Should().NotBeNullOrEmpty();
        result.SecurityCode.Length.Should().BeGreaterThan(20); // DigestValue is typically base64 encoded
    }

    [Fact]
    public async Task SignXmlAsync_SignedXmlShouldContainDigitalSignatureElements()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result = await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        result.SignedXml.Should().Contain("<Signature");
        result.SignedXml.Should().Contain("<SignedInfo");
        result.SignedXml.Should().Contain("<SignatureValue");
        result.SignedXml.Should().Contain("<KeyInfo");
        result.SignedXml.Should().Contain("<DigestValue"); // This is the SecurityCode
    }

    [Fact]
    public async Task SignXmlAsync_WithInvalidCertificatePassword_ShouldThrowException()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF><Encabezado><IdDoc><eNCF>E310000000001</eNCF></IdDoc></Encabezado></ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "correct_password"),
            Password = "wrong_password" // Incorrect password
        };

        // Act
        Func<Task> act = async () => await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        await act.Should().ThrowAsync<CryptographicException>();
    }

    [Fact]
    public async Task SignXmlAsync_WithInvalidXml_ShouldThrowException()
    {
        // Arrange
        var invalidXml = "This is not valid XML";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        Func<Task> act = async () => await _sut.SignXmlAsync(invalidXml, certModel);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task SignXmlAsync_SameXmlTwice_SecurityCodeShouldBeTheSame()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result1 = await _sut.SignXmlAsync(xmlContent, certModel);

        // Sign again with a different certificate
        var certificate2 = GenerateTestCertificate();
        var certModel2 = new DigitalCertificateModel
        {
            Content = certificate2.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };
        var result2 = await _sut.SignXmlAsync(xmlContent, certModel2);

        // Assert
        // SecurityCode (DigestValue) is a hash of the XML content, so it should be the same
        // regardless of which certificate is used to sign
        result1.SignedXml.Should().NotBeNullOrEmpty();
        result2.SignedXml.Should().NotBeNullOrEmpty();
        result1.SecurityCode.Should().Be(result2.SecurityCode); // Same XML = same digest

        // But the SignatureValue (actual signature) should be different
        result1.SignedXml.Should().NotBe(result2.SignedXml); // Different certificates = different signature
    }

    [Fact]
    public async Task SignXmlAsync_SecurityCodeShouldBeBase64Encoded()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ECF>
    <Encabezado>
        <IdDoc>
            <eNCF>E310000000001</eNCF>
        </IdDoc>
    </Encabezado>
</ECF>";

        var certificate = GenerateTestCertificate();
        var certModel = new DigitalCertificateModel
        {
            Content = certificate.Export(X509ContentType.Pfx, "test"),
            Password = "test"
        };

        // Act
        var result = await _sut.SignXmlAsync(xmlContent, certModel);

        // Assert
        // Check if SecurityCode is valid Base64
        try
        {
            Convert.FromBase64String(result.SecurityCode);
            Assert.True(true); // Is valid Base64
        }
        catch (FormatException)
        {
            Assert.Fail("SecurityCode should be valid Base64 encoded string");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generates a self-signed certificate for testing purposes.
    /// </summary>
    private X509Certificate2 GenerateTestCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Test Certificate",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1)
        );

        // Export and re-import to make it persistable
        var pfxData = certificate.Export(X509ContentType.Pfx, "test");
        return new X509Certificate2(pfxData, "test", X509KeyStorageFlags.Exportable);
    }

    #endregion
}

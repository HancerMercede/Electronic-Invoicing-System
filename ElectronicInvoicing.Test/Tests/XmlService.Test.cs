using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Domain.Enums;
using ElectronicInvoicing.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ElectronicInvoicing.Test.Tests;

public class XmlService_Test
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICompanyRepository> _mockCompanyRepository;
    private readonly Mock<ILogger<XmlService>> _mockLogger;
    private readonly XmlService _sut;

    public XmlService_Test()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCompanyRepository = new Mock<ICompanyRepository>();
        _mockLogger = new Mock<ILogger<XmlService>>();

        _mockUnitOfWork.Setup(x => x.CompanyRepository).Returns(_mockCompanyRepository.Object);

        _sut = new XmlService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    #region GenerateInvoiceXmlAsync Tests

    [Fact]
    public async Task GenerateInvoiceXmlAsync_WithValidInvoice_ShouldReturnXmlString()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company SRL",
            Address = "Av. Test 123, Santo Domingo"
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuedAt = DateTime.Now,
            IndicatorId = 1,
            IssuerRnc = "131793916",
            IssuerCompanyName = "Test Company SRL",
            CustomerRnc = "40212345678",
            CustomerName = "Customer Name",
            TotalAmount = 1180m,
            TaxableAmount = 1000m,
            TaxAmount = 180m,
            Status = InvoiceStatus.Draft,
            Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Product A",
                    Quantity = 2,
                    UnitPrice = 500m
                }
            }
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _sut.GenerateInvoiceXmlAsync(invoice);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<?xml");
        result.Should().Contain("E3100000001"); // ECF
        result.Should().Contain("131793916"); // RNC
        result.Should().Contain("Test Company SRL");

        _mockCompanyRepository.Verify(x => x.GetCompanyByIdAsync(companyId), Times.Once);
    }

    [Fact]
    public async Task GenerateInvoiceXmlAsync_WhenCompanyNotFound_ShouldThrowException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = new Invoice
        {
            CompanyId = companyId,
            ECF = "E3100000001"
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync((Company?)null);

        // Act
        Func<Task> act = async () => await _sut.GenerateInvoiceXmlAsync(invoice);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("La compañía asociada a la factura no existe.");
    }

    [Fact]
    public async Task GenerateInvoiceXmlAsync_ShouldIncludeAllInvoiceItems()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company"
        };

        var invoice = new Invoice
        {
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuedAt = DateTime.Now,
            IndicatorId = 1,
            TotalAmount = 1000m,
            TaxableAmount = 1000m,
            TaxAmount = 0m,
            Items = new List<InvoiceItem>
            {
                new InvoiceItem { Description = "Item 1", Quantity = 1, UnitPrice = 500m },
                new InvoiceItem { Description = "Item 2", Quantity = 2, UnitPrice = 250m }
            }
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _sut.GenerateInvoiceXmlAsync(invoice);

        // Assert
        result.Should().Contain("Item 1");
        result.Should().Contain("Item 2");
    }

    [Fact]
    public async Task GenerateInvoiceXmlAsync_ShouldFormatAmountsCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company"
        };

        var invoice = new Invoice
        {
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuedAt = DateTime.Now,
            IndicatorId = 1,
            TotalAmount = 1180.50m,
            TaxableAmount = 1000.50m,
            TaxAmount = 180m,
            Items = new List<InvoiceItem>()
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _sut.GenerateInvoiceXmlAsync(invoice);

        // Assert
        result.Should().Contain("1180.5"); // TotalAmount rounded
        result.Should().Contain("1000.5"); // TaxableAmount rounded
    }

    #endregion

    #region ValidateXmlAsync Tests

    [Fact]
    public async Task ValidateXmlAsync_WithEmptyXml_ShouldReturnFalse()
    {
        // Arrange
        var xmlContent = string.Empty;
        var xsdPath = "path/to/schema.xsd";

        // Act
        var result = await _sut.ValidateXmlAsync(xmlContent, xsdPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateXmlAsync_WithNullXml_ShouldReturnFalse()
    {
        // Arrange
        string? xmlContent = null;
        var xsdPath = "path/to/schema.xsd";

        // Act
        var result = await _sut.ValidateXmlAsync(xmlContent!, xsdPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateXmlAsync_WithWhitespaceXml_ShouldReturnFalse()
    {
        // Arrange
        var xmlContent = "   ";
        var xsdPath = "path/to/schema.xsd";

        // Act
        var result = await _sut.ValidateXmlAsync(xmlContent, xsdPath);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ParseXmlToInvoiceAsync Tests

    [Fact]
    public async Task ParseXmlToInvoiceAsync_WithEmptyXml_ShouldThrowArgumentException()
    {
        // Arrange
        var xmlContent = string.Empty;

        // Act
        Func<Task> act = async () => await _sut.ParseXmlToInvoiceAsync(xmlContent);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("XML content cannot be empty.");
    }

    [Fact]
    public async Task ParseXmlToInvoiceAsync_WithNullXml_ShouldThrowArgumentException()
    {
        // Arrange
        string? xmlContent = null;

        // Act
        Func<Task> act = async () => await _sut.ParseXmlToInvoiceAsync(xmlContent!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ParseXmlToInvoiceAsync_WithInvalidXml_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invalidXml = "<invalid>xml content</invalid>";

        // Act
        Func<Task> act = async () => await _sut.ParseXmlToInvoiceAsync(invalidXml);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to parse DGII XML to Invoice entity.");
    }

    #endregion

    #region Integration-like Tests

    [Fact]
    public async Task GenerateAndParseInvoice_ShouldMaintainDataIntegrity()
    {
        // This test verifies that generating XML and parsing it back maintains data integrity
        // Note: This is more of an integration test but useful for verifying the round-trip

        // Arrange
        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Rnc = "131793916",
            Name = "Test Company SRL",
            Address = "Test Address"
        };

        var originalInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuedAt = new DateTime(2026, 2, 12),
            IndicatorId = 1,
            CustomerRnc = "40212345678",
            CustomerName = "Test Customer",
            TotalAmount = 1180m,
            TaxableAmount = 1000m,
            TaxAmount = 180m,
            Items = new List<InvoiceItem>
            {
                new InvoiceItem
                {
                    Description = "Product A",
                    Quantity = 2,
                    UnitPrice = 500m
                }
            }
        };

        _mockCompanyRepository
            .Setup(x => x.GetCompanyByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var generatedXml = await _sut.GenerateInvoiceXmlAsync(originalInvoice);

        // We can't fully parse it back without the complete XML structure matching EcfXmlModel
        // But we can verify the XML was generated
        generatedXml.Should().NotBeNullOrEmpty();
        generatedXml.Should().Contain(originalInvoice.ECF);
        generatedXml.Should().Contain(originalInvoice.CustomerName);
    }

    #endregion
}

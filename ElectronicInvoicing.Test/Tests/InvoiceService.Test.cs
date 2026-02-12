using ElectronicInvoicing.Application.Services;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ElectronicInvoicing.Test.Tests;

public class InvoiceService_Test
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly InvoiceService _sut; // System Under Test

    public InvoiceService_Test()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();

        // Setup UnitOfWork to return mocked repository
        _mockUnitOfWork.Setup(x => x.InvoiceRepository).Returns(_mockInvoiceRepository.Object);

        _sut = new InvoiceService(_mockUnitOfWork.Object);
    }

    #region GetInvoiceByIdAsync Tests

    [Fact]
    public async Task GetInvoiceByIdAsync_WhenInvoiceExists_ShouldReturnInvoice()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var expectedInvoice = new Invoice
        {
            Id = invoiceId,
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuerRnc = "131793916",
            IssuerCompanyName = "Test Company",
            TotalAmount = 1000m,
            Status = InvoiceStatus.Draft
        };

        _mockInvoiceRepository
            .Setup(x => x.GetInvoiceById(companyId, invoiceId, It.IsAny<bool>()))
            .ReturnsAsync(expectedInvoice);

        // Act
        var result = await _sut.GetInvoiceByIdAsync(companyId, invoiceId, false);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invoiceId);
        result.CompanyId.Should().Be(companyId);
        result.ECF.Should().Be("E3100000001");

        _mockInvoiceRepository.Verify(x => x.GetInvoiceById(companyId, invoiceId, false), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_WhenInvoiceDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        _mockInvoiceRepository
            .Setup(x => x.GetInvoiceById(companyId, invoiceId, It.IsAny<bool>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var result = await _sut.GetInvoiceByIdAsync(companyId, invoiceId, false);

        // Assert
        result.Should().BeNull();
        _mockInvoiceRepository.Verify(x => x.GetInvoiceById(companyId, invoiceId, false), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceByIdAsync_WithTrackChangesTrue_ShouldPassCorrectParameter()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice { Id = invoiceId, CompanyId = companyId };

        _mockInvoiceRepository
            .Setup(x => x.GetInvoiceById(companyId, invoiceId, true))
            .ReturnsAsync(invoice);

        // Act
        await _sut.GetInvoiceByIdAsync(companyId, invoiceId, true);

        // Assert
        _mockInvoiceRepository.Verify(x => x.GetInvoiceById(companyId, invoiceId, true), Times.Once);
    }

    #endregion

    #region GetAllInvoices Tests

    [Fact]
    public async Task GetAllInvoices_WhenInvoicesExist_ShouldReturnList()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoices = new List<Invoice>
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ECF = "E3100000001",
                TotalAmount = 1000m,
                Status = InvoiceStatus.Draft
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ECF = "E3100000002",
                TotalAmount = 2000m,
                Status = InvoiceStatus.Sent
            }
        };

        _mockInvoiceRepository
            .Setup(x => x.GetAllInvoice(companyId, It.IsAny<bool>()))
            .ReturnsAsync(invoices);

        // Act
        var result = await _sut.GetAllInvoices(companyId, false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.CompanyId == companyId);
        _mockInvoiceRepository.Verify(x => x.GetAllInvoice(companyId, false), Times.Once);
    }

    [Fact]
    public async Task GetAllInvoices_WhenNoInvoices_ShouldReturnEmptyList()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _mockInvoiceRepository
            .Setup(x => x.GetAllInvoice(companyId, It.IsAny<bool>()))
            .ReturnsAsync(new List<Invoice>());

        // Act
        var result = await _sut.GetAllInvoices(companyId, false);

        // Assert
        result.Should().BeEmpty();
        _mockInvoiceRepository.Verify(x => x.GetAllInvoice(companyId, false), Times.Once);
    }

    [Fact]
    public async Task GetAllInvoices_ShouldFilterByCompanyId()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();

        var invoices = new List<Invoice>
        {
            new Invoice { Id = Guid.NewGuid(), CompanyId = companyId, ECF = "E3100000001" }
        };

        _mockInvoiceRepository
            .Setup(x => x.GetAllInvoice(companyId, false))
            .ReturnsAsync(invoices);

        // Act
        var result = await _sut.GetAllInvoices(companyId, false);

        // Assert
        result.Should().OnlyContain(i => i.CompanyId == companyId);
        result.Should().NotContain(i => i.CompanyId == otherCompanyId);
    }

    #endregion

    #region CreateInvoiceAsync Tests

    [Fact]
    public async Task CreateInvoiceAsync_WithValidInvoice_ShouldReturnCreatedInvoice()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuerRnc = "131793916",
            IssuerCompanyName = "Test Company",
            CustomerRnc = "40212345678",
            CustomerName = "Customer Name",
            TotalAmount = 1180m,
            TaxableAmount = 1000m,
            TaxAmount = 180m,
            Status = InvoiceStatus.Draft
        };

        _mockInvoiceRepository
            .Setup(x => x.CreateInvoiceAsync(companyId, It.IsAny<Invoice>()))
            .ReturnsAsync((Guid _, Invoice inv) => inv);

        // Act
        var result = await _sut.CreateInvoiceAsync(companyId, invoice);

        // Assert
        result.Should().NotBeNull();
        result.CompanyId.Should().Be(companyId);
        result.ECF.Should().Be("E3100000001");
        result.TotalAmount.Should().Be(1180m);

        _mockInvoiceRepository.Verify(x => x.CreateInvoiceAsync(companyId, invoice), Times.Once);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ShouldPreserveInvoiceData()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ECF = "E3100000001",
            IssuerRnc = "131793916",
            IssuerCompanyName = "Test Company",
            TotalAmount = 1000m,
            Status = InvoiceStatus.Draft,
            IssuedAt = DateTime.UtcNow
        };

        _mockInvoiceRepository
            .Setup(x => x.CreateInvoiceAsync(companyId, It.IsAny<Invoice>()))
            .ReturnsAsync((Guid _, Invoice inv) => inv);

        // Act
        var result = await _sut.CreateInvoiceAsync(companyId, invoice);

        // Assert
        result.Should().BeEquivalentTo(invoice);
    }

    #endregion

    #region UpdateInvoiceAsync Tests

    [Fact]
    public async Task UpdateInvoiceAsync_WithValidInvoice_ShouldReturnUpdatedInvoice()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            ECF = "E3100000001",
            IssuerRnc = "131793916",
            IssuerCompanyName = "Updated Company",
            TotalAmount = 2000m,
            Status = InvoiceStatus.Draft
        };

        _mockInvoiceRepository
            .Setup(x => x.UpdateAsync(invoiceId, It.IsAny<Invoice>()))
            .ReturnsAsync((Guid _, Invoice inv) => inv);

        // Act
        var result = await _sut.UpdateInvoiceAsync(invoiceId, invoice);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invoiceId);
        result.IssuerCompanyName.Should().Be("Updated Company");
        result.TotalAmount.Should().Be(2000m);

        _mockInvoiceRepository.Verify(x => x.UpdateAsync(invoiceId, invoice), Times.Once);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_ShouldPreserveInvoiceId()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            ECF = "E3100000001",
            TotalAmount = 1500m
        };

        _mockInvoiceRepository
            .Setup(x => x.UpdateAsync(invoiceId, It.IsAny<Invoice>()))
            .ReturnsAsync((Guid id, Invoice inv) => { inv.Id = id; return inv; });

        // Act
        var result = await _sut.UpdateInvoiceAsync(invoiceId, invoice);

        // Assert
        result.Id.Should().Be(invoiceId);
    }

    #endregion

    #region DeleteInvoiceAsync Tests

    [Fact]
    public async Task DeleteInvoiceAsync_WithValidInvoice_ShouldCallRepository()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ECF = "E3100000001",
            Status = InvoiceStatus.Draft
        };

        _mockInvoiceRepository
            .Setup(x => x.DeleteInvoiceAsync(companyId, invoice))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteInvoiceAsync(companyId, invoice);

        // Assert
        _mockInvoiceRepository.Verify(x => x.DeleteInvoiceAsync(companyId, invoice), Times.Once);
    }

    [Fact]
    public async Task DeleteInvoiceAsync_ShouldNotThrowException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId
        };

        _mockInvoiceRepository
            .Setup(x => x.DeleteInvoiceAsync(companyId, invoice))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _sut.DeleteInvoiceAsync(companyId, invoice);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion
}

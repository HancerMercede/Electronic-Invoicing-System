using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.DTOs.Invoice;
using ElectronicInvoicing.Domain.Entities;
using ElectronicInvoicing.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicInvoicing.API.Endpoints;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .WithOpenApi();

        // GET: /api/invoices?companyId={companyId}
        group.MapGet("/", GetAllInvoices)
            .WithName("GetAllInvoices")
            .WithSummary("Get all invoices for a company")
            .WithDescription("Retrieves a list of all invoices for the specified company")
            .Produces<IEnumerable<InvoiceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        // GET: /api/invoices/{id}?companyId={companyId}
        group.MapGet("/{id:guid}", GetInvoiceById)
            .WithName("GetInvoiceById")
            .WithSummary("Get invoice by ID")
            .WithDescription("Retrieves a specific invoice by its unique identifier")
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // POST: /api/invoices?companyId={companyId}
        group.MapPost("/", CreateInvoice)
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice")
            .WithDescription("Creates a new invoice with items for the specified company")
            .Produces<InvoiceDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        // PUT: /api/invoices/{id}?companyId={companyId}
        group.MapPut("/{id:guid}", UpdateInvoice)
            .WithName("UpdateInvoice")
            .WithSummary("Update an existing invoice")
            .WithDescription("Updates invoice information (only for Draft status)")
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // DELETE: /api/invoices/{id}?companyId={companyId}
        group.MapDelete("/{id:guid}", DeleteInvoice)
            .WithName("DeleteInvoice")
            .WithSummary("Delete an invoice")
            .WithDescription("Deletes an invoice (only for Draft status)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetAllInvoices(
        [FromQuery] Guid companyId,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            if (companyId == Guid.Empty)
                return Results.BadRequest(new { Message = "Company ID is required." });

            var invoices = await serviceManager.InvoiceService.GetAllInvoices(companyId, trackChanges: false);

            var invoicesDto = invoices.Select(MapToDto).ToList();

            return Results.Ok(invoicesDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while retrieving invoices: {ex.Message}");
        }
    }

    private static async Task<IResult> GetInvoiceById(
        Guid id,
        [FromQuery] Guid companyId,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            if (companyId == Guid.Empty)
                return Results.BadRequest(new { Message = "Company ID is required." });

            var invoice = await serviceManager.InvoiceService.GetInvoiceByIdAsync(companyId, id, trackChanges: false);

            if (invoice == null)
                return Results.NotFound(new { Message = $"Invoice with ID {id} not found." });

            var invoiceDto = MapToDto(invoice);

            return Results.Ok(invoiceDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while retrieving the invoice: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateInvoice(
        [FromQuery] Guid companyId,
        [FromBody] CreateInvoiceDto createDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            if (companyId == Guid.Empty)
                return Results.BadRequest(new { Message = "Company ID is required." });

            // Get company to populate issuer information
            var company = await serviceManager.CompanyService.GetCompanyByIdAsync(companyId);
            if (company == null)
                return Results.NotFound(new { Message = $"Company with ID {companyId} not found." });

            // Calculate totals from items
            var items = createDto.Items.Select(item =>
            {
                var lineTotal = (item.Quantity * item.UnitPrice) - item.Discount + item.TaxAmount;
                return new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    ProductCode = item.ProductCode,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TaxAmount = item.TaxAmount,
                    LineTotal = lineTotal,
                    CompanyId = companyId
                };
            }).ToList();

            var totalAmount = items.Sum(i => i.LineTotal);
            var discountAmount = items.Sum(i => i.Discount);
            var taxAmount = items.Sum(i => i.TaxAmount);
            var taxableAmount = totalAmount - createDto.ExemptAmount - taxAmount;

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                ECF = createDto.ECF,
                IndicatorId = createDto.IndicatorId,
                IssuedAt = DateTime.UtcNow,
                ExpirationDate = createDto.ExpirationDate,
                IssuerRnc = company.Rnc,
                IssuerCompanyName = company.Name,
                CustomerRnc = createDto.CustomerRnc,
                CustomerName = createDto.CustomerName,
                TotalAmount = totalAmount,
                TaxableAmount = taxableAmount,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                ExemptAmount = createDto.ExemptAmount,
                Status = InvoiceStatus.Draft,
                CompanyId = companyId,
                Items = items
            };

            var createdInvoice = await serviceManager.InvoiceService.CreateInvoiceAsync(companyId, invoice);

            var invoiceDto = MapToDto(createdInvoice);

            return Results.Created($"/api/invoices/{invoiceDto.Id}", invoiceDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while creating the invoice: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateInvoice(
        Guid id,
        [FromQuery] Guid companyId,
        [FromBody] UpdateInvoiceDto updateDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            if (companyId == Guid.Empty)
                return Results.BadRequest(new { Message = "Company ID is required." });

            var invoice = await serviceManager.InvoiceService.GetInvoiceByIdAsync(companyId, id, trackChanges: true);

            if (invoice == null)
                return Results.NotFound(new { Message = $"Invoice with ID {id} not found." });

            // Only allow updates for Draft invoices
            if (invoice.Status != InvoiceStatus.Draft)
                return Results.BadRequest(new { Message = "Only invoices in Draft status can be updated." });

            // Update properties
            invoice.ExpirationDate = updateDto.ExpirationDate;
            invoice.CustomerRnc = updateDto.CustomerRnc;
            invoice.CustomerName = updateDto.CustomerName;
            invoice.ExemptAmount = updateDto.ExemptAmount;

            // Update items if provided
            if (updateDto.Items != null && updateDto.Items.Any())
            {
                // Remove old items
                invoice.Items.Clear();

                // Add new items
                var items = updateDto.Items.Select(item =>
                {
                    var lineTotal = (item.Quantity * item.UnitPrice) - item.Discount + item.TaxAmount;
                    return new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        ProductCode = item.ProductCode,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.Discount,
                        TaxAmount = item.TaxAmount,
                        LineTotal = lineTotal,
                        CompanyId = companyId
                    };
                }).ToList();

                foreach (var item in items)
                {
                    invoice.Items.Add(item);
                }

                // Recalculate totals
                var totalAmount = items.Sum(i => i.LineTotal);
                var discountAmount = items.Sum(i => i.Discount);
                var taxAmount = items.Sum(i => i.TaxAmount);
                var taxableAmount = totalAmount - updateDto.ExemptAmount - taxAmount;

                invoice.TotalAmount = totalAmount;
                invoice.TaxableAmount = taxableAmount;
                invoice.TaxAmount = taxAmount;
                invoice.DiscountAmount = discountAmount;
            }

            var updatedInvoice = await serviceManager.InvoiceService.UpdateInvoiceAsync(id, invoice);

            var invoiceDto = MapToDto(updatedInvoice);

            return Results.Ok(invoiceDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while updating the invoice: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteInvoice(
        Guid id,
        [FromQuery] Guid companyId,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            if (companyId == Guid.Empty)
                return Results.BadRequest(new { Message = "Company ID is required." });

            var invoice = await serviceManager.InvoiceService.GetInvoiceByIdAsync(companyId, id, trackChanges: false);

            if (invoice == null)
                return Results.NotFound(new { Message = $"Invoice with ID {id} not found." });

            // Only allow deletion for Draft invoices
            if (invoice.Status != InvoiceStatus.Draft)
                return Results.BadRequest(new { Message = "Only invoices in Draft status can be deleted." });

            await serviceManager.InvoiceService.DeleteInvoiceAsync(companyId, invoice);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while deleting the invoice: {ex.Message}");
        }
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            ECF = invoice.ECF,
            IndicatorId = invoice.IndicatorId,
            IssuedAt = invoice.IssuedAt,
            ExpirationDate = invoice.ExpirationDate,
            IssuerRnc = invoice.IssuerRnc,
            IssuerCompanyName = invoice.IssuerCompanyName,
            CustomerRnc = invoice.CustomerRnc,
            CustomerName = invoice.CustomerName,
            TotalAmount = invoice.TotalAmount,
            TaxableAmount = invoice.TaxableAmount,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            ExemptAmount = invoice.ExemptAmount,
            Status = invoice.Status,
            DgiiTrackId = invoice.DgiiTrackId,
            SecurityCode = invoice.SecurityCode,
            DgiiResponseCode = invoice.DgiiResponseCode,
            RejectionReason = invoice.RejectionReason,
            SentAt = invoice.SentAt,
            ValidatedAt = invoice.ValidatedAt,
            QrContent = invoice.QrContent,
            CompanyId = invoice.CompanyId,
            Items = invoice.Items.Select(item => new InvoiceItemDto
            {
                Id = item.Id,
                ProductCode = item.ProductCode,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                TaxAmount = item.TaxAmount,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }
}

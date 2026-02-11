using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.DTOs.Company;
using ElectronicInvoicing.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicInvoicing.API.Endpoints;

public static class CompanyEndpoints
{
    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies")
            .WithTags("Companies")
            .WithOpenApi();

        // GET: /api/companies
        group.MapGet("/", GetAllCompanies)
            .WithName("GetAllCompanies")
            .WithSummary("Get all companies")
            .WithDescription("Retrieves a list of all registered companies")
            .Produces<IEnumerable<CompanyDto>>(StatusCodes.Status200OK);

        // GET: /api/companies/{id}
        group.MapGet("/{id:guid}", GetCompanyById)
            .WithName("GetCompanyById")
            .WithSummary("Get company by ID")
            .WithDescription("Retrieves a specific company by its unique identifier")
            .Produces<CompanyDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST: /api/companies
        group.MapPost("/", CreateCompany)
            .WithName("CreateCompany")
            .WithSummary("Create a new company")
            .WithDescription("Creates a new company with optional digital certificate")
            .Produces<CompanyDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT: /api/companies/{id}
        group.MapPut("/{id:guid}", UpdateCompany)
            .WithName("UpdateCompany")
            .WithSummary("Update an existing company")
            .WithDescription("Updates company information including certificate if provided")
            .Produces<CompanyDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // DELETE: /api/companies/{id}
        group.MapDelete("/{id:guid}", DeleteCompany)
            .WithName("DeleteCompany")
            .WithSummary("Delete a company")
            .WithDescription("Soft deletes a company by setting IsActive to false")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAllCompanies(
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var companies = await serviceManager.CompanyService.GetAllCompanies(trackChanges: false);

            var companiesDto = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Rnc = c.Rnc,
                Name = c.Name,
                CommercialName = c.CommercialName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                IsElectronicIssuer = c.IsElectronicIssuer,
                CertificateExpiration = c.CertificateExpiration,
                HasDigitalCertificate = c.DigitalCertificate != null && c.DigitalCertificate.Length > 0,
                ApiClientId = c.ApiClientId,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive
            });

            return Results.Ok(companiesDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while retrieving companies: {ex.Message}");
        }
    }

    private static async Task<IResult> GetCompanyById(
        Guid id,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var company = await serviceManager.CompanyService.GetCompanyByIdAsync(id);

            if (company == null)
                return Results.NotFound(new { Message = $"Company with ID {id} not found." });

            var companyDto = new CompanyDto
            {
                Id = company.Id,
                Rnc = company.Rnc,
                Name = company.Name,
                CommercialName = company.CommercialName,
                Address = company.Address,
                PhoneNumber = company.PhoneNumber,
                IsElectronicIssuer = company.IsElectronicIssuer,
                CertificateExpiration = company.CertificateExpiration,
                HasDigitalCertificate = company.DigitalCertificate != null && company.DigitalCertificate.Length > 0,
                ApiClientId = company.ApiClientId,
                CreatedAt = company.CreatedAt,
                IsActive = company.IsActive
            };

            return Results.Ok(companyDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while retrieving the company: {ex.Message}");
        }
    }

    private static async Task<IResult> CreateCompany(
        [FromBody] CreateCompanyDto createDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            // Convert Base64 certificate to byte array if provided
            byte[]? certificateBytes = null;
            if (!string.IsNullOrWhiteSpace(createDto.DigitalCertificateBase64))
            {
                try
                {
                    certificateBytes = Convert.FromBase64String(createDto.DigitalCertificateBase64);
                }
                catch (FormatException)
                {
                    return Results.BadRequest(new { Message = "Invalid Base64 format for digital certificate." });
                }
            }

            var company = new Company
            {
                Id = Guid.NewGuid(),
                Rnc = createDto.Rnc,
                Name = createDto.Name,
                CommercialName = createDto.CommercialName,
                Address = createDto.Address,
                PhoneNumber = createDto.PhoneNumber,
                IsElectronicIssuer = createDto.IsElectronicIssuer,
                DigitalCertificate = certificateBytes,
                CertificatePassword = createDto.CertificatePassword,
                CertificateExpiration = createDto.CertificateExpiration,
                ApiClientId = createDto.ApiClientId,
                ApiClientSecret = createDto.ApiClientSecret,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdCompany = await serviceManager.CompanyService.CreateCompanyAsync(company);

            var companyDto = new CompanyDto
            {
                Id = createdCompany.Id,
                Rnc = createdCompany.Rnc,
                Name = createdCompany.Name,
                CommercialName = createdCompany.CommercialName,
                Address = createdCompany.Address,
                PhoneNumber = createdCompany.PhoneNumber,
                IsElectronicIssuer = createdCompany.IsElectronicIssuer,
                CertificateExpiration = createdCompany.CertificateExpiration,
                HasDigitalCertificate = createdCompany.DigitalCertificate != null && createdCompany.DigitalCertificate.Length > 0,
                ApiClientId = createdCompany.ApiClientId,
                CreatedAt = createdCompany.CreatedAt,
                IsActive = createdCompany.IsActive
            };

            return Results.Created($"/api/companies/{companyDto.Id}", companyDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while creating the company: {ex.Message}");
        }
    }

    private static async Task<IResult> UpdateCompany(
        Guid id,
        [FromBody] UpdateCompanyDto updateDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var company = await serviceManager.CompanyService.GetCompanyByIdAsync(id);

            if (company == null)
                return Results.NotFound(new { Message = $"Company with ID {id} not found." });

            // Update properties
            company.Name = updateDto.Name;
            company.CommercialName = updateDto.CommercialName;
            company.Address = updateDto.Address;
            company.PhoneNumber = updateDto.PhoneNumber;
            company.IsElectronicIssuer = updateDto.IsElectronicIssuer;
            company.ApiClientId = updateDto.ApiClientId;
            company.ApiClientSecret = updateDto.ApiClientSecret;
            company.IsActive = updateDto.IsActive;

            // Update certificate if provided
            if (!string.IsNullOrWhiteSpace(updateDto.DigitalCertificateBase64))
            {
                try
                {
                    company.DigitalCertificate = Convert.FromBase64String(updateDto.DigitalCertificateBase64);
                    company.CertificatePassword = updateDto.CertificatePassword;
                    company.CertificateExpiration = updateDto.CertificateExpiration;
                }
                catch (FormatException)
                {
                    return Results.BadRequest(new { Message = "Invalid Base64 format for digital certificate." });
                }
            }

            // Update company through service
            var updatedCompany = await serviceManager.CompanyService.UpdateCompanyAsync(company);

            var companyDto = new CompanyDto
            {
                Id = updatedCompany.Id,
                Rnc = updatedCompany.Rnc,
                Name = updatedCompany.Name,
                CommercialName = updatedCompany.CommercialName,
                Address = updatedCompany.Address,
                PhoneNumber = updatedCompany.PhoneNumber,
                IsElectronicIssuer = updatedCompany.IsElectronicIssuer,
                CertificateExpiration = updatedCompany.CertificateExpiration,
                HasDigitalCertificate = updatedCompany.DigitalCertificate != null && updatedCompany.DigitalCertificate.Length > 0,
                ApiClientId = updatedCompany.ApiClientId,
                CreatedAt = updatedCompany.CreatedAt,
                IsActive = updatedCompany.IsActive
            };

            return Results.Ok(companyDto);
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while updating the company: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteCompany(
        Guid id,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var company = await serviceManager.CompanyService.GetCompanyByIdAsync(id);

            if (company == null)
                return Results.NotFound(new { Message = $"Company with ID {id} not found." });

            await serviceManager.CompanyService.DeleteCompanyAsync(company);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while deleting the company: {ex.Message}");
        }
    }
}

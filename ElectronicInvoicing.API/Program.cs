using System.Text;
using ElectronicInvoicing.API.Endpoints;
using ElectronicInvoicing.API.Helpers;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();

// Configure Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ElectronicInvoicing API",
        Version = "v1",
        Description = "API for Electronic Invoicing (DGII Dominican Republic) - Nexus.Store Standard"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureTenantService();
builder.Services.ConfigureUnitOfWork();
builder.Services.ConfigureDgiiService();
builder.Services.ConfiguringServiceManager();
builder.Services.ConfiguredHttpClient(builder.Configuration);
builder.Services.ConfigureSignatureService();
builder.Services.ConfigureInvoiceProcessorService();
builder.Services.ConfigureXmlService();

var app = builder.Build();

if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
    app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoint groups
app.MapAuthEndpoints();
app.MapCompanyEndpoints();
app.MapInvoiceEndpoints();

app.MapGet("/Greetings", () => "Hello I am online!!");

app.MapPost("/api/invoices/{id:guid}/process", async (
        Guid id,
        [FromQuery] Guid companyId,
        [FromServices] IInvoiceProcessorService processor) =>
    {
        try
        {
            var success = await processor.ProcessAndSendAsync(companyId, id);

            return success
                ? Results.Ok(new { Message = "Invoice processed and successfully sent to DGII." })
                : Results.BadRequest(new
                    { Message = "The invoice was rejected by DGII. Please check the rejection logs." });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred while processing the invoice: {ex.Message}");
        }
    })
    .WithName("ProcessInvoice");

app.Run();


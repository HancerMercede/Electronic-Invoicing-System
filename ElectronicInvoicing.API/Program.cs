using ElectronicInvoicing.API.Helpers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.ConfigureContext(builder.Configuration);
builder.Services.ConfigureTenantService();
builder.Services.ConfigureUnitOfWork();
builder.Services.ConfiguringServiceManager();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/Greetings", () => "Hello I am online!!");

app.Run();


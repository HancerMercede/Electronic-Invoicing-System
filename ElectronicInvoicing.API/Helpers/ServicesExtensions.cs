using ElectronicInvoicing.Infrastructure.Context;
using ElectronicInvoicing.Infrastructure.Contracts;
using ElectronicInvoicing.Infrastructure.Repositories;
using ElectronicInvoicing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.API.Helpers;

public static class ServicesExtensions
{
    public static void ConfigureContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                x => x.MigrationsAssembly("ElectronicInvoicing.Infrastructure"));
        });
    }

    public static void ConfigureTenantService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantService, TenantService>();
    }
}
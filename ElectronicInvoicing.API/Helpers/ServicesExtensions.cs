using ElectronicInvoicing.Infrastructure.Context;
using ElectronicInvoicing.Infrastructure.Contracts;
using ElectronicInvoicing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.API.Helpers;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureContext(IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly("ElectronicInvoicing.Infrastructure"));
            });
        }

        public void ConfigureTenantService()
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantService, TenantService>();
        }
    }
}
using ElectronicInvoicing.Application.Services;
using ElectronicInvoicing.Domain.Contracts;
using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Infrastructure.Context;
using ElectronicInvoicing.Infrastructure.Repositories;
using ElectronicInvoicing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.API.Helpers;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureContext(IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(opt =>
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

        public void ConfigureUnitOfWork()
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public void ConfiguringServiceManager()
        {
            services.AddScoped<IServiceManager, ServiceManager>();
        }
    }
}
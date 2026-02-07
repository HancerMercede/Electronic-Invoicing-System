using System.Net;
using System.Net.Http.Headers;
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

        public void ConfigureDgiiService()
        {
            services.AddScoped<IDgiiService, DgiiService>();
        }

        public void ConfigureInvoiceProcessorService()
        {
            services.AddScoped<IInvoiceProcessorService, InvoiceProcessorService>();
        }

        public void ConfiguredHttpClient(IConfiguration configuration)
        {
            services.AddHttpClient("DgiiClient",opt =>
            {
                var baseUrl = configuration["DgiiRemoteServices:AuthUrl"]; 
                opt.BaseAddress = new Uri(baseUrl!);
                
                opt.DefaultRequestVersion = HttpVersion.Version11;
                opt.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                
                opt.DefaultRequestHeaders.Accept.Clear();
                opt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                opt.DefaultRequestHeaders.Add("User-Agent", "ElectronicInvoicingApp/1.0");
            });
            
            services.AddScoped<IDgiiService, DgiiService>();
        }

        public void ConfigureSignatureService()
        {
            services.AddScoped<ISignatureService, SignatureService>();
        }
        
        public void ConfigureXmlService()
        {
            services.AddScoped<IXmlService, XmlService>();
        }
    }
}
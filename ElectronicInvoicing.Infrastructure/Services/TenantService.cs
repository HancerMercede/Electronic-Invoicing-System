using ElectronicInvoicing.Infrastructure.Contracts;
using Microsoft.AspNetCore.Http;

namespace ElectronicInvoicing.Infrastructure.Services;

public class TenantService(IHttpContextAccessor httpContextAccessor): ITenantService
{
    public Guid GetTenantId() // Asegúrate que el nombre coincida con la interfaz
    {
        var context = httpContextAccessor.HttpContext 
                      ?? throw new InvalidOperationException("HttpContext no disponible.");

        // 1. Intentar con Claims
        var claimValue = context.User?.FindFirst("CompanyId")?.Value;
        if (Guid.TryParse(claimValue, out var tenantId)) 
        {
            return tenantId;
        }

        // 2. Intentar con Headers
        // Nota: No usamos 'var' aquí porque tenantId ya fue declarada arriba
        var headerValue = context.Request.Headers["X-Tenant-Id"].ToString();
        if (Guid.TryParse(headerValue, out tenantId)) 
        {
            return tenantId;
        }

        throw new UnauthorizedAccessException("Tenant no identificado.");
    }
}
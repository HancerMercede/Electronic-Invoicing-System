namespace ElectronicInvoicing.Infrastructure.Contracts;

public interface ITenantService
{
    Guid GetTenantId();
}
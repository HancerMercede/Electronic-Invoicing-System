namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface ITenantService
{
    Guid GetTenantId();
}
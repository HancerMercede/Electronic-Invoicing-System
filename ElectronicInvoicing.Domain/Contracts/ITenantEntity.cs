namespace ElectronicInvoicing.Domain.Contracts;

public interface ITenantEntity
{
    public Guid CompanyId { get; set; }
}
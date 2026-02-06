using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;

namespace ElectronicInvoicing.Application.Services;

public class ServiceManager(IUnitOfWork unitOfWork): IServiceManager
{
   private readonly Lazy<ICompanyService> _companyService = new(() => new CompanyService(unitOfWork)); 
   private readonly Lazy<IInvoiceService> _invoiceService = new(() => new InvoiceService(unitOfWork));
   private readonly Lazy<IInvoiceItemService>  _invoiceItemService = new(() => new InvoiceItemService(unitOfWork));
   
   public ICompanyService CompanyService => _companyService.Value;
   public IInvoiceService InvoiceService => _invoiceService.Value;
   public IInvoiceItemService InvoiceItemService => _invoiceItemService.Value;
}
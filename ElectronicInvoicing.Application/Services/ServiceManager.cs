using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using Microsoft.Extensions.Configuration;

namespace ElectronicInvoicing.Application.Services;

public class ServiceManager(IUnitOfWork unitOfWork, IConfiguration configuration, IEncryptionService encryptionService): IServiceManager
{
   private readonly Lazy<ICompanyService> _companyService = new(() => new CompanyService(unitOfWork, encryptionService));
   private readonly Lazy<IInvoiceService> _invoiceService = new(() => new InvoiceService(unitOfWork));
   private readonly Lazy<IInvoiceItemService>  _invoiceItemService = new(() => new InvoiceItemService(unitOfWork));
   private readonly Lazy<IAuthService> _authService = new(() => new AuthService(unitOfWork, configuration));
   private readonly Lazy<IUserService> _userService = new(() => new UserService(unitOfWork));



   public ICompanyService CompanyService => _companyService.Value;
   public IInvoiceService InvoiceService => _invoiceService.Value;
   public IInvoiceItemService InvoiceItemService => _invoiceItemService.Value;
   public IAuthService AuthService => _authService.Value;
   public IUserService UserService => _userService.Value;
}
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.ServicesContracts;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<User>> GetAllUsersByCompanyAsync(Guid companyId);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(User user);
}

using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Domain.Contracts.RepositoryContracts;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<IEnumerable<User>> GetAllByCompanyIdAsync(Guid companyId);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
}

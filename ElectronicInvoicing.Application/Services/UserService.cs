using ElectronicInvoicing.Domain.Contracts.RepositoryContracts;
using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.Entities;

namespace ElectronicInvoicing.Application.Services;

public class UserService(IUnitOfWork unitOfWork) : IUserService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await unitOfWork.UserRepository.GetByIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetAllUsersByCompanyAsync(Guid companyId)
    {
        return await unitOfWork.UserRepository.GetAllByCompanyIdAsync(companyId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        var result = await unitOfWork.UserRepository.CreateAsync(user);
        await unitOfWork.SaveChanges();
        return result;
    }

    public async Task UpdateUserAsync(User user)
    {
        await unitOfWork.UserRepository.UpdateAsync(user);
        await unitOfWork.SaveChanges();
    }

    public async Task DeleteUserAsync(User user)
    {
        await unitOfWork.UserRepository.DeleteAsync(user);
        await unitOfWork.SaveChanges();
    }
}

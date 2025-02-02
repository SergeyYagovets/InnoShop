using UserManagement.Domain.Entities;

namespace UserManagement.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
    
    Task<bool> AnyUsersAsync();
    Task<User?> GetByEmailAsync(string email);
}
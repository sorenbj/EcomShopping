using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task<User?> GetWithRolesAsync(int userId);
}

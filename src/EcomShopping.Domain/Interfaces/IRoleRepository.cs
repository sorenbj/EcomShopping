using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
}

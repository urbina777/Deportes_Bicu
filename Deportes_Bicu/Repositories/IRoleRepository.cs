using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IRoleRepository
    {
        Task<IReadOnlyList<Role>> GetAllWithPermissionsAsync();
        Task <Role?> GetByIdWithPermissionAsync (int id);
        Task<Role?> GetByNameAsync(string name);
        Task<Role> CreateAsync (string  name);
        Task<bool> AssingPermissionToRoleAsync(int roleId, int permissionId);
        Task <bool> RemovePermissionFromRoleAsync (int roleId, int permissionId);
    }
}

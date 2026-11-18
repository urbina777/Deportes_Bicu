using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IPermissionRepository
    {
        Task<DataTableResponseDto<PermissionDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<PermissionDto>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int Id);
        Task<Permission?> GetByNameAsync(string name);
        Task<Permission> CreateAsync(string name, string? description);
        Task<bool> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(int Id);
        Task<bool> IsAssignedToUsuarioAsync(int permissionId);
    }
}

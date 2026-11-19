using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class RolePermissionService(IRoleRepository _roleRepo, IPermissionRepository permissionRepo, IUsuarioRepository userRepo) : IRolePermissionService
    {
        public async Task<(bool Success, string? ErrorMessage)> AssignPermissionToRoleAsync(int roleld, int permissionId)
        {
           var role = await _roleRepo.GetByIdWithPermissionAsync(roleld);
            if (role == null)
                return (false, "Rol no encontrado");

            var permission = await permissionRepo.GetByIdAsync(permissionId);
            if (permission == null)
                return (false, "Permiso no encontrado");

            var assigned = await _roleRepo.AssingPermissionToRoleAsync(roleld, permissionId);
            if (!assigned)
                return (false, "El rol ya tiene asignado ese permiso");
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AssignPermissionToUserAsync(int userId, AssingUserPermission dto)
        {
            var user = await userRepo.GetUsuarioByIdWithPermissionAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado");

            var permission = await permissionRepo.GetByIdAsync(dto.PermissionId);
            if(permission == null)
                return (false, "Permiso no encontrado");

            await userRepo.AssingPermissionToUsuarioAsync(userId, dto.PermissionId, dto.IsGranted);
            return (true, null);
        }

        public async Task<(bool Success, PermissionDto? Result, string? ErrorMessage)> CreatePermissionAsync(SavePermissionDto dto)
        {
            var name = dto.Name.Trim().ToLowerInvariant();
            var existing = await permissionRepo.GetByNameAsync(name);
            if (existing != null)
                return (false, null, "Ya existe un permiso con ese nombre");

            var permission = await permissionRepo.CreateAsync(name, dto.Descripcion);
            return (true, new PermissionDto
            {
                Id = permission.Id,
                Name = name,
                Description = permission.Descripcion
            }, null);
        }

        public async Task<(bool Success, RoleWithPermissionDto? Result, string? ErrorMessage)> CreateRoleAsync(CreateRolDto dto)
        {
            var existing = await _roleRepo.GetByNameAsync(dto.Name.Trim());
            if (existing != null) return (false, null, "Ya existe un Rol con ese nombre");
            var role = await _roleRepo.CreateAsync(dto.Name);
            return (true, new RoleWithPermissionDto
            {
                Id = role.Id,
                Name = dto.Name,
                Permissions = []
            }, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeletePermissionAsync(int id)
        {
            var permission = await permissionRepo.GetByIdAsync(id);
            if (permission == null)
                return (false, "Permiso no encontrado");

            if(await permissionRepo.IsAssignedToUsuarioAsync(id))
                return (false, "No se puede eliminar un Permiso asignado a un Usuario");
            await permissionRepo.DeleteAsync(id);
            return (true, null);
        }

        public async Task<IReadOnlyList<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await permissionRepo.GetAllAsync();
            return permissions.Select(p => new PermissionDto
            { 
                Id = p.Id, 
                Name = p.Name,
                Description = p.Description,
            }).ToList();
        }

        public async Task<IReadOnlyList<RoleWithPermissionDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepo.GetAllWithPermissionsAsync();
            return roles.Select(MapRole).ToList();
        }

        public async Task<IReadOnlyList<UserListItemDto>> GetAllUsersAsync()
        {
            var users = await userRepo.GetAllUsuariosAsync();
            return users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                UserName = u.userName,
                Email = u.Email,
                RoleName = u.Role?.Descripcion ?? string.Empty,
            }).ToList();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            var permission = await permissionRepo.GetByIdAsync(id);
            if(permission == null)
                return null;
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Nombre,
                Description = permission.Descripcion
            };
        }

        public async Task<DataTableResponseDto<PermissionDto>> GetPermissionsPagedAsync(DataTableRequestDto request)
        {
            return await permissionRepo.GetPagedAsync(request);
        }

        public async Task<RoleWithPermissionDto?> GetRoleByIdAsync(int roleId)
        {
            var role = await _roleRepo.GetByIdWithPermissionAsync(roleId);
            return role == null? null : MapRole(role);
        }

        public async Task<UserPermissionDto?> GetUserPermissionsAsync(int userId)
        {
            var user = await userRepo.GetUsuarioByIdWithPermissionAsync(userId);
            if(user == null) 
                return null;
            return new UserPermissionDto
            {
                UserId = user.Id,
                UserName = user.userName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Descripcion ?? string.Empty,
                EffectivePermission = EffectivePermission.FromUser(user).ToList(),
                DirectPermission = user.UsuarioPermission.Select(up => new UserPermissionOverrideDto
                {
                    PermissionId = up.PermissionId,
                    PermissionName = up.Permission.Nombre,
                    IsGranted = up.isGranted,
                }).ToList(),
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> RemovePermissionFromRoleAsync(int roleld, int permissionId)
        {
            var remove = await _roleRepo.RemovePermissionFromRoleAsync(roleld, permissionId);
            if (!remove)
                return (false, "El Rol no tiene asignado este permiso");
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveUserPermissionAsync(int userId, int permissionId)
        {
            var remove = await userRepo.RemovePermissionFromUsuarioAsync(userId, permissionId);
            if (!remove)
                return (false, "El Usuario no tiene una asignado para este permiso");
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePermissionAsync(SavePermissionDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Permiso Invalido");
            var permission = await permissionRepo.GetByIdAsync(dto.Id.Value);
            if(permission == null) 
                return (false, "Permiso no encontrado");
            var name = dto.Name.Trim().ToLowerInvariant();
            var duplicate = await permissionRepo.GetByNameAsync(name);
            if (duplicate != null && duplicate.Id != permission.Id)
                return (false, "Existe permiso con ese nombre");
            permission.Nombre = name;
            permission.Descripcion = dto.Descripcion?.Trim();
            await permissionRepo.UpdateAsync(permission);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserRoleAsync(int userId, int roleId)
        {
            var update = await userRepo.UpdateUsuarioRolAsync(userId, roleId);
            if (!update)
                return (false, "Usuario o Rol no encontrado");
            return(true, null);


        }

        private static RoleWithPermissionDto MapRole(Role role) => new()
        {
            Id = role.Id,
            Name = role.Descripcion,
            Permissions = role.RolePermission.Select(rp => new PermissionDto
            {
                Id = rp.PermissionId,
                Name = rp.Permission.Nombre,
                Description = rp.Permission.Descripcion,
            }).OrderBy(p => p.Name).ToList(),
        };
    }
}

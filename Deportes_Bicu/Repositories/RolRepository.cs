using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class RolRepository(AppDbContext context) : IRoleRepository
    {
        public async Task<bool> AssingPermissionToRoleAsync(int roleId, int permissionId)
        {
            var exists = await context.RolePermission.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (exists) return false;
            context.RolePermission.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Role> CreateAsync(string name)
        {
            var role = new Role { Descripcion = name.Trim() };
            var entry = await context.Role.AddAsync(role);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<IReadOnlyList<Role>> GetAllWithPermissionsAsync()
        {
            return await context.Role.
                Include(r => r.RolePermission)
                .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdWithPermissionAsync(int id)
        {
            return await context.Role.
                Include(r => r.RolePermission)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await context.Role.FirstOrDefaultAsync(r => r.Descripcion == name);
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermission = await context.RolePermission.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
            if (rolePermission == null) return false;

            context.RolePermission.Remove(rolePermission);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

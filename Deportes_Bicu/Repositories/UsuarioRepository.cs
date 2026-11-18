using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class UsuarioRepository(AppDbContext context) : IUsuarioRepository
    {
        private IQueryable<Usuario> UserWithPermission =>
            context.Usuario
            .Include(u => u.Role)
            .ThenInclude(r => r.RolePermission)
            .ThenInclude(rp => rp.Permission)
            .Include(up => up.UsuarioPermission)
            .ThenInclude(rp => rp.Permission);
        public async Task<bool> AssingPermissionToUsuarioAsync(int usuarioId, int permissionId, bool isGranted)
        {
            var userPermission = await context.UsuarioPermission.FirstOrDefaultAsync(up => up.UsuarioId == usuarioId && up.PermissionId == permissionId);

            if (userPermission != null)
            {
                userPermission.isGranted = isGranted;
            }
            else
            {
                context.UsuarioPermission.Add(new UsuarioPermission
                {
                    UsuarioId = usuarioId,
                    PermissionId = permissionId,
                    isGranted = isGranted
                });
            }
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            var entry = await context.Usuario.AddAsync(usuario);
            return entry.Entity;
        }

        public async Task<IReadOnlyList<Usuario>> GetAllUsuariosAsync()
        {
            return await context.Usuario.Include(u => u.Role).OrderBy(u => u.userName).ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            return await UserWithPermission.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetUsuarioByIdWithPermissionAsync(int Id)
        {
            return await UserWithPermission.FirstOrDefaultAsync(u => u.Id == Id);
        }

        public async Task<Usuario?> GetUsuarioByRefreshTokenAsync(string refreshToken)
        {
            return await UserWithPermission.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<Usuario?> GetUsuarioByUserName(string userName)
        {
            return await UserWithPermission.FirstOrDefaultAsync(u => u.userName == userName);
        }

        public async Task<bool> RemovePermissionFromUsuarioAsync(int usuarioId, int permissionId)
        {
            var userPermission = await context.UsuarioPermission.FirstOrDefaultAsync(up => up.UsuarioId == usuarioId && up.PermissionId == permissionId);
            if (userPermission == null) return false;

            context.UsuarioPermission.Remove(userPermission);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task<bool> UpdateUsuarioRolAsync(int usuarioId, int roleId)
        {
            var user = await context.Usuario.FindAsync(usuarioId);
            if (user == null) return false;

            var roleExists = await context.Role.AnyAsync(r => r.Id == roleId);
            if (!roleExists) return false;
            user.RoleId = roleId;
            await context.SaveChangesAsync();
            return true;
        }

        public bool ValidatePassword(Usuario usuario, string password)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, usuario.Password);
        }
    }
}

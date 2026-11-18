using Deportes_Bicu.Models;

namespace Deportes_Bicu.Helper
{
    public class EffectivePermission
    {
        public static IReadOnlyList<string> FromUser(Usuario usuario)
        {
            var fromRole = usuario.Role?.RolePermission?.Select(rp => rp.Permission.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

            var granted = usuario.UsuarioPermission?.Where(up => up.isGranted)
                .Select(up  => up.Permission.Nombre)
                .ToList() ?? [];

            var denied = usuario.UsuarioPermission?
                .Where(up => !up.isGranted)
                .Select(up => up.Permission.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

            foreach(var permission in granted)
            {
                fromRole.Add(permission);
            }

            fromRole.ExceptWith(denied);

            return fromRole.OrderBy(p => p).ToList();
        }
    }
}

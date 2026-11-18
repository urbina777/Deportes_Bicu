using System.Security.Claims;

namespace Deportes_Bicu.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public const string PermisionClaimType = "permission";


        public static bool HasPermision(this ClaimsPrincipal user, string permision)
        {
            return user.Claims.Any(c => c.Type == PermisionClaimType
            && string.Equals(c.Value, permision, StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permission)
        {
            return permission.Any(user.HasPermision);
        }

        //Id del usuario autenticado (claim NameIdentifier); null si no es valido
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        }

        public static IReadOnlyList<string> GetPermission(this ClaimsPrincipal user)
        {
            return user.Claims
                .Where(c => c.Type == PermisionClaimType)
                .Select(c => c.Value)
                .OrderBy(p => p)
                .ToList();
        }
    }
}

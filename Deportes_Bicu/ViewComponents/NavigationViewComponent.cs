using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Repositories;
using System.Security.Claims;

namespace Deportes_Bicu.ViewComponents
{
    public class NavigationViewComponent(IUsuarioRepository _userRepo): ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User as ClaimsPrincipal;
            var permission = user?.GetPermission() ?? [];
            if(user?.Identity?.IsAuthenticated == true && int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
            {
                var dbUser = await _userRepo.GetUsuarioByIdWithPermissionAsync(id);
                if(dbUser != null)
                    permission = EffectivePermission.FromUser(dbUser);
            }
            return View(permission);
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;
using Deportes_Bicu.Service;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Deportes_Bicu.Controllers
{
    public class AdminController(IUsuarioRepository _usuarioRepository, IRolePermissionService _rolePermissionService) : Controller
    {
        private async Task<IReadOnlyList<string>> GetEffecctivePermissionAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return User.GetPermission();
            var dbUser = await _usuarioRepository.GetUsuarioByIdWithPermissionAsync(userId);
            return dbUser != null
                ? EffectivePermission.FromUser(dbUser)
                : User.GetPermission();

        }

        [Authorize(Policy = "admin.panel")]
        public IActionResult Index()
        {
            ViewBag.Permissions = User.GetPermission();
            return View();
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpPost]
        public async Task<IActionResult> GetPermissionData([FromBody] DataTableRequestDto request)
        {
            var result = await _rolePermissionService.GetPermissionsPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "permiso.ver")]
        public async Task<IActionResult> Permissions()
        {
            var effective = await GetEffecctivePermissionAsync();
            ViewBag.CanCreate = effective.Contains("permiso.crear", StringComparer.OrdinalIgnoreCase);
            ViewBag.CanUpdate = effective.Any(p =>
            string.Equals(p, "permiso.actualizar", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p, "permiso.crear", StringComparison.OrdinalIgnoreCase));
            ViewBag.CanDelete = effective.Any(p =>
            string.Equals(p, "permiso.borrar", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p, "permiso.crear", StringComparison.OrdinalIgnoreCase));
            return View();
        }

        [Authorize(Policy = "permiso.ver")]
        [HttpGet]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _rolePermissionService.GetPermissionByIdAsync(id);
            if (permission == null)
                return NotFound(new { message = "Permiso no encontrado" });
            return Ok(permission);
        }

        [Authorize(Policy = "permiso.crear")]
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Revise los datos del formulario" });
            var (success, _, errorMessage) = await _rolePermissionService.CreatePermissionAsync(dto);
            if (!success)
                return BadRequest(new { success = false, message = errorMessage });
            return Ok(new { success = true, message = $"Permiso '{dto.Name}' creado correctamente" });

        }

        [Authorize(Policy = "permiso.actualizar")]
        [HttpPost]
        public async Task<IActionResult> UpdatePermission([FromBody] SavePermissionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Revise los datos del formulario" });
            var (success, errorMessage) = await _rolePermissionService.UpdatePermissionAsync(dto);
            if (!success)
                return BadRequest(new { success = false, message = errorMessage });
            return Ok(new { success = true, message = $"Permiso '{dto.Name}' actualizado correctamente" });

        }

        [Authorize(Policy = "permiso.borrar")]
        [HttpPost]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var (success, errorMessage) = await _rolePermissionService.DeletePermissionAsync(id);
            if (!success)
                return BadRequest(new { success = false, message = errorMessage });
            return Ok(new { success = true, message = $"Permiso borrado correctamente" });
        }

        [Authorize(Policy = "rol.ver")]
        public async Task<IActionResult> Roles()
        {
            var roles = await _rolePermissionService.GetAllRolesAsync();
            return View(roles);
        }

        [Authorize(Policy = "rol.crear")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRolDto dto)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _rolePermissionService.GetAllRolesAsync();
                ViewBag.CreateError = "El nombre del rol es obligatorio";
                return View("Roles", roles);
            }

            var (success, _, errorMessage) = await _rolePermissionService.CreateRoleAsync(dto);
            if (!success)
            {
                var roles = await _rolePermissionService.GetAllRolesAsync();
                ViewBag.CreateError = errorMessage;
                return View("Roles", roles);
            }
            TempData["Success"] = $"Rol '{dto.Name}' creado correctamente";
            return RedirectToAction(nameof(Roles));

        }

        [Authorize(Policy = "rol.asignar")]
        public async Task<IActionResult> RoleDetails(int id)
        {
            var role = await _rolePermissionService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound();
            var allPermission = await _rolePermissionService.GetAllPermissionsAsync();
            ViewBag.AllPermissions = allPermission;
            return View(role);
        }

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        public async Task<IActionResult> AssignPermissionToRole(int roleId, int permissionId)
        {
            var (success, errorMessage) = await _rolePermissionService.AssignPermissionToRoleAsync(roleId, permissionId);
            return Ok(new { success, message = success ? "Permiso Asignado" : errorMessage });
        }

        [Authorize(Policy = "rol.asignar")]
        [HttpPost]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            var (success, errorMessage) = await _rolePermissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return Ok(new { success, message = success ? "Permiso Removido" : errorMessage });
        }

        [Authorize(Policy = "usuario.ver")]
        public async Task<IActionResult> Users()
        {
            var users = await _rolePermissionService.GetAllUsersAsync();
            return View(users);
        }

        [Authorize(Policy = "usuario.asignar")]
        public async Task<IActionResult> UserPermissions(int id)
        {
            var user = await _rolePermissionService.GetUserPermissionsAsync(id);
            if (user == null)
                return NotFound();
            var allPermission = await _rolePermissionService.GetAllPermissionsAsync();
            var allRoles = await _rolePermissionService.GetAllRolesAsync();
            ViewBag.AllPermissions = allPermission;
            ViewBag.AllRoles = allRoles;
            ViewBag.CanChangeRole = User.HasPermision("usuario.cambiar_rol");
            return View(user);
        }

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        public async Task<IActionResult> AssignPermissionToUser(int userId, [FromBody] AssingUserPermission dto)
        {
            var (success, errorMessage) = await _rolePermissionService.AssignPermissionToUserAsync(userId, dto);
            return Json(new { success, message = success ? (dto.IsGranted ? "Permiso concedido al usuario" : "Permiso denegado al usuario") : errorMessage });
        }

        [Authorize(Policy = "usuario.asignar")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserPermission(int userId, int permissionId)
        {
            var (success, errorMessage) = await _rolePermissionService.RemoveUserPermissionAsync(userId, permissionId);
            return Json(new { success, message = success ? "Asignacion directa removida" : errorMessage });

        }

        [Authorize(Policy = "usuario.cambiar_rol")]
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleDto dto)
        {
            var (success, errorMessage) = await _rolePermissionService.UpdateUserRoleAsync(userId, dto.RoleId);
            return Json(new { success, message = success ? "Rol actualizado" : errorMessage });
        }
    }
}

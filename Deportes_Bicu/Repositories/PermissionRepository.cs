using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class PermissionRepository(AppDbContext context) : IPermissionRepository
    {
        public async Task<Permission> CreateAsync(string name, string? description)
        {
            var permission = new Permission
            {
                Nombre = name.Trim().ToLowerInvariant(),
                Descripcion = description?.Trim()
            };
            var entry = await context.AddAsync(permission);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var permission = await context.Permission.FindAsync(id);
            if (permission == null) return false;
            context.Permission.Remove(permission);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<PermissionDto>> GetAllAsync()
        {
            return await context.Permission.OrderBy(x => x.Nombre)
                .Select(x => new PermissionDto
                {
                    Id = x.Id,
                    Name = x.Nombre,
                    Description = x.Descripcion
                }).ToListAsync();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await context.Permission.FindAsync(id);
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await context.Permission.FirstOrDefaultAsync(x => x.Nombre == name);
        }

        public async Task<DataTableResponseDto<PermissionDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Permission.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Nombre.Contains(search)
                || (c.Descripcion != null && c.Descripcion.Contains(search)));
            }
            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "description" => orderDir == "desc" ? query.OrderByDescending(c => c.Descripcion) : query.OrderBy(c => c.Descripcion),
                "id" => orderDir == "desc" ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre)
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(c => new PermissionDto
                {
                    Id = c.Id,
                    Name = c.Nombre,
                    Description = c.Descripcion
                }).ToListAsync();

            return new DataTableResponseDto<PermissionDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> IsAssignedToUsuarioAsync(int permissionId)
        {
            return await context.UsuarioPermission.AnyAsync(up => up.PermissionId == permissionId);

        }

        public async Task<bool> UpdateAsync(Permission permission)
        {
            context.Permission.Update(permission);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

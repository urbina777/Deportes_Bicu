using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class FacultadRepository(AppDbContext context) : IFacultadRepository
    {
        public async Task<Facultad> CreateAsync(Facultad facultad)
        {
            var entry = await context.Facultad.AddAsync(facultad);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var facultad = await context.Facultad.FindAsync(Id);
            if (facultad == null) return false;

            context.Facultad.Remove(facultad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<Facultad>> GetAllAsync()
        {
            return await context.Facultad.OrderBy(f => f.Nombre).ToListAsync();
        }

        public async Task<Facultad?> GetByIdAsync(int Id)
        {
            return await context.Facultad.FindAsync(Id);
        }

        public async Task<DataTableResponseDto<FacultadTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Facultad.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Nombre.Contains(search)
                || (f.Descripcion != null && f.Descripcion.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "descripcion" => orderDir == "desc" ? query.OrderByDescending(f => f.Descripcion) : query.OrderBy(f => f.Descripcion),
                "totalcarreras" => orderDir == "desc" ?
                query.OrderByDescending(f => f.Carrera.Count) : query.OrderBy(f => f.Carrera.Count),
                "id" => orderDir == "desc" ? query.OrderByDescending(f => f.Id) : query.OrderBy(f => f.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(f => f.Nombre) : query.OrderBy(f => f.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(f => new FacultadTableDto
                {
                    Id = f.Id,
                    Nombre = f.Nombre,
                    Descripcion = f.Descripcion,
                    TotalCarreras = f.Carrera.Count
                }).ToListAsync();

            return new DataTableResponseDto<FacultadTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> HasAnyCarreraAsync(int Id)
        {
            return await context.Carrera.AnyAsync(c => c.FacultadId == Id);
        }

        public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null)
        {
            return await context.Facultad.AnyAsync(f => f.Nombre == nombre
            && (!excludeId.HasValue || f.Id != excludeId.Value));
        }

        public async Task<bool> UpdateAsync(Facultad facultad)
        {
            context.Facultad.Update(facultad);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

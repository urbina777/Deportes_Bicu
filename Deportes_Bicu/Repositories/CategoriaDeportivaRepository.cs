using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class CategoriaDeportivaRepository(AppDbContext context) : ICategoriaDeportivaRepository
    {
        public async Task<CategoriaDeportiva> CreateAsync(CategoriaDeportiva entidad)
        {
            var entry = await context.CategoriaDeportiva.AddAsync(entidad);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var entidad = await context.CategoriaDeportiva.FindAsync(Id);
            if (entidad == null) return false;

            context.CategoriaDeportiva.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<CategoriaDeportiva?> GetByIdAsync(int Id)
        {
            return await context.CategoriaDeportiva.FindAsync(Id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        {
            var query = context.CategoriaDeportiva.AsQueryable();

            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new OpcionDto { Id = x.Id, Texto = x.Nombre + " (" + x.EdadMinima.ToString() + "-" + x.EdadMaxima.ToString() + " años)" })
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<CategoriaDeportivaTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.CategoriaDeportiva.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Nombre.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "edadminima" => orderDir == "desc" ? query.OrderByDescending(x => x.EdadMinima) : query.OrderBy(x => x.EdadMinima),
                "edadmaxima" => orderDir == "desc" ? query.OrderByDescending(x => x.EdadMaxima) : query.OrderBy(x => x.EdadMaxima),
                "id" => orderDir == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(x => x.Nombre) : query.OrderBy(x => x.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(x => new CategoriaDeportivaTableDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    EdadMinima = x.EdadMinima,
                    EdadMaxima = x.EdadMaxima
                }).ToListAsync();

            return new DataTableResponseDto<CategoriaDeportivaTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null)
        {
            return await context.CategoriaDeportiva.AnyAsync(x => x.Nombre == nombre
            && (!excludeId.HasValue || x.Id != excludeId.Value));
        }

        public async Task<bool> TieneRelacionadosAsync(int Id)
        {
            return await context.DeporteCategoria.AnyAsync(d => d.CategoriaDeportivaId == Id);
        }

        public async Task<bool> UpdateAsync(CategoriaDeportiva entidad)
        {
            context.CategoriaDeportiva.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }

    }
}

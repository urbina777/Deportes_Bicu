using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class PeriodoRepository(AppDbContext context) : IPeriodoRepository
    {
        public async Task<Periodo> CreateAsync(Periodo entidad)
        {
            var entry = await context.Periodo.AddAsync(entidad);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var entidad = await context.Periodo.FindAsync(Id);
            if (entidad == null) return false;

            context.Periodo.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Periodo?> GetByIdAsync(int Id)
        {
            return await context.Periodo.FindAsync(Id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        {
            var query = context.Periodo.AsQueryable();
            if (soloDisponibles)
                query = query.Where(x => x.Estado == Estados.PeriodoAbierto);
            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new OpcionDto { Id = x.Id, Texto = x.Nombre })
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<PeriodoTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Periodo.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Nombre.Contains(search) || x.Estado.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "fechainicio" => orderDir == "desc" ? query.OrderByDescending(x => x.FechaInicio) : query.OrderBy(x => x.FechaInicio),
                "fechafin" => orderDir == "desc" ? query.OrderByDescending(x => x.FechaFin) : query.OrderBy(x => x.FechaFin),
                "estado" => orderDir == "desc" ? query.OrderByDescending(x => x.Estado) : query.OrderBy(x => x.Estado),
                "id" => orderDir == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(x => x.Nombre) : query.OrderBy(x => x.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(x => new PeriodoTableDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    FechaInicio = x.FechaInicio,
                    FechaFin = x.FechaFin,
                    Estado = x.Estado
                }).ToListAsync();

            return new DataTableResponseDto<PeriodoTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null)
        {
            return await context.Periodo.AnyAsync(x => x.Nombre == nombre
            && (!excludeId.HasValue || x.Id != excludeId.Value));
        }

        public async Task<bool> TieneRelacionadosAsync(int Id)
        {
            return await context.Inscripcion.AnyAsync(i => i.PeriodoId == Id);
        }

        public async Task<bool> UpdateAsync(Periodo entidad)
        {
            context.Periodo.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }

    }
}

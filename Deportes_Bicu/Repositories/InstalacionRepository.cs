using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class InstalacionRepository(AppDbContext context) : IInstalacionRepository
    {
        public async Task<Instalacion> CreateAsync(Instalacion entidad)
        {
            var entry = await context.Instalacion.AddAsync(entidad);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var entidad = await context.Instalacion.FindAsync(Id);
            if (entidad == null) return false;

            context.Instalacion.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Instalacion?> GetByIdAsync(int Id)
        {
            return await context.Instalacion.FindAsync(Id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        {
            var query = context.Instalacion.AsQueryable();
            if (soloDisponibles)
                query = query.Where(x => x.Estado == Estados.Activo);
            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new OpcionDto { Id = x.Id, Texto = x.Nombre })
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<InstalacionTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Instalacion.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Nombre.Contains(search) || (x.Ubicacion != null && x.Ubicacion.Contains(search)) || x.Estado.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "ubicacion" => orderDir == "desc" ? query.OrderByDescending(x => x.Ubicacion) : query.OrderBy(x => x.Ubicacion),
                "capacidad" => orderDir == "desc" ? query.OrderByDescending(x => x.Capacidad) : query.OrderBy(x => x.Capacidad),
                "estado" => orderDir == "desc" ? query.OrderByDescending(x => x.Estado) : query.OrderBy(x => x.Estado),
                "id" => orderDir == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(x => x.Nombre) : query.OrderBy(x => x.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(x => new InstalacionTableDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Ubicacion = x.Ubicacion,
                    Capacidad = x.Capacidad,
                    Estado = x.Estado
                }).ToListAsync();

            return new DataTableResponseDto<InstalacionTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null)
        {
            return await context.Instalacion.AnyAsync(x => x.Nombre == nombre
            && (!excludeId.HasValue || x.Id != excludeId.Value));
        }

        public async Task<bool> TieneRelacionadosAsync(int Id)
        {
            return await context.Horario.AnyAsync(h => h.InstalacionId == Id);
        }

        public async Task<bool> UpdateAsync(Instalacion entidad)
        {
            context.Instalacion.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MaxOcupacionAsync(int instalacionId)
        {
            var ocupaciones = await context.InscripcionHorario
                .Where(ih => ih.Horario.InstalacionId == instalacionId
                    && ih.Inscripcion.Estado == Estados.InscripcionActiva)
                .GroupBy(ih => new { ih.HorarioId, ih.Inscripcion.PeriodoId })
                .Select(g => g.Count())
                .ToListAsync();
            return ocupaciones.Count == 0 ? 0 : ocupaciones.Max();
        }
    }
}

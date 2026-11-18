using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class DeporteRepository(AppDbContext context) : IDeporteRepository
    {
        public async Task<Deporte> CreateAsync(Deporte entidad)
        {
            var entry = await context.Deporte.AddAsync(entidad);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var entidad = await context.Deporte.FindAsync(Id);
            if (entidad == null) return false;

            context.Deporte.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Deporte?> GetByIdAsync(int Id)
        {
            return await context.Deporte.FindAsync(Id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        {
            var query = context.Deporte.AsQueryable();
            if (soloDisponibles)
                query = query.Where(x => x.Estado == Estados.Activo);
            return await query
                .OrderBy(x => x.Nombre)
                .Select(x => new OpcionDto { Id = x.Id, Texto = x.Nombre })
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<DeporteTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Deporte.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Nombre.Contains(search) || (x.Descripcion != null && x.Descripcion.Contains(search)) || x.Estado.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "descripcion" => orderDir == "desc" ? query.OrderByDescending(x => x.Descripcion) : query.OrderBy(x => x.Descripcion),
                "estado" => orderDir == "desc" ? query.OrderByDescending(x => x.Estado) : query.OrderBy(x => x.Estado),
                "id" => orderDir == "desc" ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(x => x.Nombre) : query.OrderBy(x => x.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(x => new DeporteTableDto
                {
                    Id = x.Id,
                    Nombre = x.Nombre,
                    Descripcion = x.Descripcion,
                    Estado = x.Estado
                }).ToListAsync();

            return new DataTableResponseDto<DeporteTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> NombreExisteAsync(string nombre, int? excludeId = null)
        {
            return await context.Deporte.AnyAsync(x => x.Nombre == nombre
            && (!excludeId.HasValue || x.Id != excludeId.Value));
        }

        public async Task<bool> TieneRelacionadosAsync(int Id)
        {
            return await context.Inscripcion.AnyAsync(i => i.DeporteId == Id) || await context.EntrenadorDeporte.AnyAsync(e => e.DeporteId == Id) || await context.DeporteCategoria.AnyAsync(d => d.DeporteId == Id);
        }

        public async Task<bool> UpdateAsync(Deporte entidad)
        {
            context.Deporte.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<AsignacionesDto> GetCategoriasAsync(int deporteId)
        {
            var asignadas = await context.DeporteCategoria
                .Where(dc => dc.DeporteId == deporteId)
                .OrderBy(dc => dc.CategoriaDeportiva.Nombre)
                .Select(dc => new
                {
                    dc.Id,
                    dc.CategoriaDeportiva.Nombre,
                    dc.CategoriaDeportiva.EdadMinima,
                    dc.CategoriaDeportiva.EdadMaxima
                }).ToListAsync();

            var disponibles = await context.CategoriaDeportiva
                .Where(c => !context.DeporteCategoria.Any(dc => dc.DeporteId == deporteId && dc.CategoriaDeportivaId == c.Id))
                .OrderBy(c => c.Nombre)
                .Select(c => new OpcionDto { Id = c.Id, Texto = c.Nombre })
                .ToListAsync();

            return new AsignacionesDto
            {
                Asignadas = asignadas.Select(a => new AsignacionDto
                {
                    Id = a.Id,
                    Texto = a.Nombre,
                    Detalle = $"{a.EdadMinima} a {a.EdadMaxima} años"
                }).ToList(),
                Disponibles = disponibles
            };
        }

        public async Task<bool> CategoriaExisteAsync(int categoriaId)
        {
            return await context.CategoriaDeportiva.AnyAsync(c => c.Id == categoriaId);
        }

        public async Task<bool> CategoriaAsignadaAsync(int deporteId, int categoriaId)
        {
            return await context.DeporteCategoria.AnyAsync(dc => dc.DeporteId == deporteId && dc.CategoriaDeportivaId == categoriaId);
        }

        public async Task AsignarCategoriaAsync(int deporteId, int categoriaId)
        {
            context.DeporteCategoria.Add(new DeporteCategoria { DeporteId = deporteId, CategoriaDeportivaId = categoriaId });
            await context.SaveChangesAsync();
        }

        public async Task<bool> CategoriaTieneInscripcionesAsync(int deporteCategoriaId)
        {
            return await context.Inscripcion.AnyAsync(i => i.DeporteCategoriaId == deporteCategoriaId);
        }

        public async Task<bool> QuitarCategoriaAsync(int deporteCategoriaId)
        {
            var dc = await context.DeporteCategoria.FindAsync(deporteCategoriaId);
            if (dc == null) return false;
            context.DeporteCategoria.Remove(dc);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

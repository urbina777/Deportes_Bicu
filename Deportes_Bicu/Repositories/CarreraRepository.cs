using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class CarreraRepository(AppDbContext context) : ICarreraRepository
    {
        public async Task<Carrera> CreateAsync(Carrera carrera)
        {
            var entry = await context.Carrera.AddAsync(carrera);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var carrera = await context.Carrera.FindAsync(Id);
            if (carrera == null) return false;

            context.Carrera.Remove(carrera);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<Carrera>> GetAllAsync()
        {
            return await context.Carrera
                .Include(c => c.Facultad)
                .OrderBy(c => c.Facultad.Nombre)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Carrera?> GetByIdAsync(int Id)
        {
            return await context.Carrera.FindAsync(Id);
        }

        public async Task<DataTableResponseDto<CarreraTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Carrera.Include(c => c.Facultad).AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Nombre.Contains(search)
                || c.Facultad.Nombre.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "facultadnombre" => orderDir == "desc" ? query.OrderByDescending(c => c.Facultad.Nombre) : query.OrderBy(c => c.Facultad.Nombre),
                "totalestudiantes" => orderDir == "desc" ?
                query.OrderByDescending(c => c.Estudiante.Count) : query.OrderBy(c => c.Estudiante.Count),
                "id" => orderDir == "desc" ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(c => new CarreraTableDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    FacultadId = c.FacultadId,
                    FacultadNombre = c.Facultad.Nombre,
                    TotalEstudiantes = c.Estudiante.Count
                }).ToListAsync();

            return new DataTableResponseDto<CarreraTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> HasAnyEstudianteAsync(int Id)
        {
            return await context.Estudiante.AnyAsync(e => e.CarreraId == Id);
        }

        public async Task<bool> NombreExisteAsync(string nombre, int facultadId, int? excludeId = null)
        {
            return await context.Carrera.AnyAsync(c => c.Nombre == nombre && c.FacultadId == facultadId
            && (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        public async Task<bool> UpdateAsync(Carrera carrera)
        {
            context.Carrera.Update(carrera);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

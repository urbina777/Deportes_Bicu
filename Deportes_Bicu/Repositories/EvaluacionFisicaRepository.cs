using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class EvaluacionFisicaRepository(AppDbContext context) : IEvaluacionFisicaRepository
    {
        public async Task<EvaluacionFisica> CreateAsync(EvaluacionFisica evaluacion)
        {
            var entry = await context.EvaluacionFisica.AddAsync(evaluacion);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var evaluacion = await context.EvaluacionFisica.FindAsync(id);
            if (evaluacion == null) return false;

            context.EvaluacionFisica.Remove(evaluacion);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EstudianteExisteAsync(int estudianteId)
        {
            return await context.Estudiante.AnyAsync(e => e.Id == estudianteId);
        }

        public async Task<EvaluacionFisica?> GetByIdAsync(int id)
        {
            return await context.EvaluacionFisica.FindAsync(id);
        }

        public async Task<IReadOnlyList<EvaluacionFisicaTableDto>> GetByEstudianteAsync(int estudianteId)
        {
            return await Proyectar(context.EvaluacionFisica.Where(e => e.EstudianteId == estudianteId))
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync()
        {
            return await context.Estudiante
                .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre)
                .Select(e => new OpcionDto { Id = e.Id, Texto = e.Apellido + ", " + e.Nombre + " (" + e.Carnet + ")" })
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<EvaluacionFisicaTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.EvaluacionFisica.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Estudiante.Nombre.Contains(search)
                || e.Estudiante.Apellido.Contains(search) || e.Estudiante.Carnet.Contains(search)
                || (e.Observaciones != null && e.Observaciones.Contains(search)));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "desc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "fecha";

            query = columnName.ToLowerInvariant() switch
            {
                "estudiantenombre" => orderDir == "desc" ? query.OrderByDescending(e => e.Estudiante.Apellido) : query.OrderBy(e => e.Estudiante.Apellido),
                "peso" => orderDir == "desc" ? query.OrderByDescending(e => e.Peso) : query.OrderBy(e => e.Peso),
                "estatura" => orderDir == "desc" ? query.OrderByDescending(e => e.Estatura) : query.OrderBy(e => e.Estatura),
                "id" => orderDir == "desc" ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(e => e.Fecha).ThenByDescending(e => e.Id) : query.OrderBy(e => e.Fecha).ThenBy(e => e.Id),
            };

            var data = await Proyectar(query)
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .ToListAsync();

            return new DataTableResponseDto<EvaluacionFisicaTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> UpdateAsync(EvaluacionFisica evaluacion)
        {
            context.EvaluacionFisica.Update(evaluacion);
            await context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<EvaluacionFisicaTableDto> Proyectar(IQueryable<EvaluacionFisica> query)
        {
            return query.Select(e => new EvaluacionFisicaTableDto
            {
                Id = e.Id,
                Fecha = e.Fecha,
                EstudianteId = e.EstudianteId,
                EstudianteNombre = e.Estudiante.Nombre + " " + e.Estudiante.Apellido,
                Carnet = e.Estudiante.Carnet,
                Peso = e.Peso,
                Estatura = e.Estatura,
                Observaciones = e.Observaciones
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class HorarioRepository(AppDbContext context) : IHorarioRepository
    {
        public async Task<Horario> CreateAsync(Horario horario)
        {
            var entry = await context.Horario.AddAsync(horario);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var horario = await context.Horario.FindAsync(id);
            if (horario == null) return false;

            context.Horario.Remove(horario);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Horario?> GetByIdAsync(int id)
        {
            return await context.Horario.FindAsync(id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetAsignacionesOpcionesAsync()
        {
            return await context.EntrenadorDeporte
                .Where(ed => ed.Deporte.Estado == Estados.Activo)
                .OrderBy(ed => ed.Deporte.Nombre).ThenBy(ed => ed.Entrenador.Apellido)
                .Select(ed => new OpcionDto
                {
                    Id = ed.Id,
                    Texto = ed.Deporte.Nombre + " - " + ed.Entrenador.Nombre + " " + ed.Entrenador.Apellido
                }).ToListAsync();
        }

        public async Task<int?> GetEntrenadorIdAsync(int entrenadorDeporteId)
        {
            return await context.EntrenadorDeporte
                .Where(ed => ed.Id == entrenadorDeporteId)
                .Select(ed => (int?)ed.EntrenadorId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> InstalacionActivaAsync(int instalacionId)
        {
            return await context.Instalacion.AnyAsync(i => i.Id == instalacionId && i.Estado == Estados.Activo);
        }


        public async Task<bool> ChoqueInstalacionAsync(string dia, TimeOnly inicio, TimeOnly fin, int instalacionId, int? excludeId = null)
        {
            var existentes = await context.Horario
                .Where(h => h.InstalacionId == instalacionId && h.Dia == dia
                    && (!excludeId.HasValue || h.Id != excludeId.Value))
                .Select(h => new { h.HoraInicio, h.HoraFin })
                .ToListAsync();
            return existentes.Any(h => Estados.Solapan(dia, inicio, fin, dia, h.HoraInicio, h.HoraFin));
        }

        public async Task<bool> ChoqueEntrenadorAsync(string dia, TimeOnly inicio, TimeOnly fin, int entrenadorId, int? excludeId = null)
        {
            var existentes = await context.Horario
                .Where(h => h.EntrenadorDeporte.EntrenadorId == entrenadorId && h.Dia == dia
                    && (!excludeId.HasValue || h.Id != excludeId.Value))
                .Select(h => new { h.HoraInicio, h.HoraFin })
                .ToListAsync();
            return existentes.Any(h => Estados.Solapan(dia, inicio, fin, dia, h.HoraInicio, h.HoraFin));
        }

        public async Task<DataTableResponseDto<HorarioTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Horario.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h => h.Dia.Contains(search)
                || h.Instalacion.Nombre.Contains(search)
                || h.EntrenadorDeporte.Deporte.Nombre.Contains(search)
                || h.EntrenadorDeporte.Entrenador.Nombre.Contains(search)
                || h.EntrenadorDeporte.Entrenador.Apellido.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "dia";

            query = columnName.ToLowerInvariant() switch
            {
                "horainicio" => orderDir == "desc" ? query.OrderByDescending(h => h.HoraInicio) : query.OrderBy(h => h.HoraInicio),
                "horafin" => orderDir == "desc" ? query.OrderByDescending(h => h.HoraFin) : query.OrderBy(h => h.HoraFin),
                "deportenombre" => orderDir == "desc" ? query.OrderByDescending(h => h.EntrenadorDeporte.Deporte.Nombre) : query.OrderBy(h => h.EntrenadorDeporte.Deporte.Nombre),
                "entrenadornombre" => orderDir == "desc" ? query.OrderByDescending(h => h.EntrenadorDeporte.Entrenador.Apellido) : query.OrderBy(h => h.EntrenadorDeporte.Entrenador.Apellido),
                "instalacionnombre" => orderDir == "desc" ? query.OrderByDescending(h => h.Instalacion.Nombre) : query.OrderBy(h => h.Instalacion.Nombre),
                "id" => orderDir == "desc" ? query.OrderByDescending(h => h.Id) : query.OrderBy(h => h.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(h => h.Dia) : query.OrderBy(h => h.Dia),
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(h => new HorarioTableDto
                {
                    Id = h.Id,
                    Dia = h.Dia,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    EntrenadorDeporteId = h.EntrenadorDeporteId,
                    DeporteNombre = h.EntrenadorDeporte.Deporte.Nombre,
                    EntrenadorNombre = h.EntrenadorDeporte.Entrenador.Nombre + " " + h.EntrenadorDeporte.Entrenador.Apellido,
                    InstalacionId = h.InstalacionId,
                    InstalacionNombre = h.Instalacion.Nombre,
                    Capacidad = h.Instalacion.Capacidad,
                    Inscritos = h.InscripcionHorario.Count(ih => ih.Inscripcion.Estado == Estados.InscripcionActiva)
                }).ToListAsync();

            return new DataTableResponseDto<HorarioTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> TieneInscripcionesAsync(int id)
        {
            return await context.InscripcionHorario.AnyAsync(ih => ih.HorarioId == id);
        }

        public async Task<bool> UpdateAsync(Horario horario)
        {
            context.Horario.Update(horario);
            await context.SaveChangesAsync();
            return true;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class AsistenciaRepository(AppDbContext context) : IAsistenciaRepository
    {
        public async Task<Asistencia> CreateAsync(Asistencia asistencia)
        {
            var entry = await context.Asistencia.AddAsync(asistencia);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asistencia = await context.Asistencia.FindAsync(id);
            if (asistencia == null) return false;

            context.Asistencia.Remove(asistencia);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAsync(int inscripcionId, DateOnly fecha, int? excludeId = null)
        {
            return await context.Asistencia.AnyAsync(a => a.InscripcionId == inscripcionId && a.Fecha == fecha
                && (!excludeId.HasValue || a.Id != excludeId.Value));
        }

        public async Task<Asistencia?> GetByIdAsync(int id)
        {
            return await context.Asistencia.FindAsync(id);
        }

        public async Task<IReadOnlyList<AsistenciaTableDto>> GetByEstudianteAsync(int estudianteId)
        {
            return await Proyectar(context.Asistencia.Where(a => a.Inscripcion.EstudianteId == estudianteId))
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<HorarioEntrenadorDto>> GetHorariosDeEntrenadorAsync(int entrenadorId)
        {
            var lista = await context.Horario
                .Where(h => h.EntrenadorDeporte.EntrenadorId == entrenadorId)
                .Select(h => new HorarioEntrenadorDto
                {
                    Id = h.Id,
                    Dia = h.Dia,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    DeporteNombre = h.EntrenadorDeporte.Deporte.Nombre,
                    InstalacionNombre = h.Instalacion.Nombre
                }).ToListAsync();

            return lista
                .OrderBy(h => Array.IndexOf(Estados.Dias, h.Dia))
                .ThenBy(h => h.HoraInicio)
                .ToList();
        }

        public async Task<HorarioEntrenadorDto?> GetHorarioDeEntrenadorAsync(int horarioId, int entrenadorId)
        {
            return await context.Horario
                .Where(h => h.Id == horarioId && h.EntrenadorDeporte.EntrenadorId == entrenadorId)
                .Select(h => new HorarioEntrenadorDto
                {
                    Id = h.Id,
                    Dia = h.Dia,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    DeporteNombre = h.EntrenadorDeporte.Deporte.Nombre,
                    InstalacionNombre = h.Instalacion.Nombre
                }).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<OpcionDto>> GetInscripcionesActivasOpcionesAsync()
        {
            return await context.Inscripcion
                .Where(i => i.Estado == Estados.InscripcionActiva)
                .OrderBy(i => i.Estudiante.Apellido).ThenBy(i => i.Estudiante.Nombre)
                .Select(i => new OpcionDto
                {
                    Id = i.Id,
                    Texto = i.Estudiante.Apellido + ", " + i.Estudiante.Nombre + " - " + i.Deporte.Nombre + " - " + i.Periodo.Nombre
                }).ToListAsync();
        }

        public async Task<InscripcionAsistenciaInfoDto?> GetInscripcionInfoAsync(int inscripcionId)
        {
            return await context.Inscripcion
                .Where(i => i.Id == inscripcionId)
                .Select(i => new InscripcionAsistenciaInfoDto
                {
                    Id = i.Id,
                    Estado = i.Estado,
                    FechaInicio = i.Periodo.FechaInicio,
                    FechaFin = i.Periodo.FechaFin,
                    Dias = i.InscripcionHorario.Select(ih => ih.Horario.Dia).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<AsistenciaInscritoDto>> GetInscritosDeHorarioAsync(int horarioId, DateOnly fecha)
        {
            return await context.InscripcionHorario
                .Where(ih => ih.HorarioId == horarioId
                    && ih.Inscripcion.Estado == Estados.InscripcionActiva
                    && ih.Inscripcion.Periodo.FechaInicio <= fecha
                    && ih.Inscripcion.Periodo.FechaFin >= fecha)
                .OrderBy(ih => ih.Inscripcion.Estudiante.Apellido).ThenBy(ih => ih.Inscripcion.Estudiante.Nombre)
                .Select(ih => new AsistenciaInscritoDto
                {
                    InscripcionId = ih.InscripcionId,
                    EstudianteNombre = ih.Inscripcion.Estudiante.Apellido + ", " + ih.Inscripcion.Estudiante.Nombre,
                    Carnet = ih.Inscripcion.Estudiante.Carnet,
                    Estado = ih.Inscripcion.Asistencia.Where(a => a.Fecha == fecha).Select(a => a.Estado).FirstOrDefault()
                }).ToListAsync();
        }

        public async Task<DataTableResponseDto<AsistenciaTableDto>> GetPagedAsync(DataTableRequestDto request, int? entrenadorId = null)
        {
            var query = context.Asistencia.AsQueryable();
            if (entrenadorId.HasValue)
            {
                query = query.Where(a => a.Inscripcion.InscripcionHorario
                    .Any(ih => ih.Horario.EntrenadorDeporte.EntrenadorId == entrenadorId.Value));
            }
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Inscripcion.Estudiante.Nombre.Contains(search)
                || a.Inscripcion.Estudiante.Apellido.Contains(search) || a.Inscripcion.Estudiante.Carnet.Contains(search)
                || a.Inscripcion.Deporte.Nombre.Contains(search) || a.Inscripcion.Periodo.Nombre.Contains(search)
                || a.Estado.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "desc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "fecha";

            query = columnName.ToLowerInvariant() switch
            {
                "estudiantenombre" => orderDir == "desc" ? query.OrderByDescending(a => a.Inscripcion.Estudiante.Apellido) : query.OrderBy(a => a.Inscripcion.Estudiante.Apellido),
                "deportenombre" => orderDir == "desc" ? query.OrderByDescending(a => a.Inscripcion.Deporte.Nombre) : query.OrderBy(a => a.Inscripcion.Deporte.Nombre),
                "periodonombre" => orderDir == "desc" ? query.OrderByDescending(a => a.Inscripcion.Periodo.Nombre) : query.OrderBy(a => a.Inscripcion.Periodo.Nombre),
                "estado" => orderDir == "desc" ? query.OrderByDescending(a => a.Estado) : query.OrderBy(a => a.Estado),
                "id" => orderDir == "desc" ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.Id) : query.OrderBy(a => a.Fecha).ThenBy(a => a.Id),
            };

            var data = await Proyectar(query)
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .ToListAsync();

            return new DataTableResponseDto<AsistenciaTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> UpdateAsync(Asistencia asistencia)
        {
            context.Asistencia.Update(asistencia);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task UpsertSesionAsync(DateOnly fecha, IEnumerable<RegistroAsistenciaDto> registros)
        {
            var lista = registros.ToList();
            var ids = lista.Select(r => r.InscripcionId).ToList();
            var existentes = await context.Asistencia
                .Where(a => a.Fecha == fecha && ids.Contains(a.InscripcionId))
                .ToListAsync();

            foreach (var registro in lista)
            {
                var actual = existentes.FirstOrDefault(a => a.InscripcionId == registro.InscripcionId);
                if (actual != null)
                    actual.Estado = registro.Estado;
                else
                    context.Asistencia.Add(new Asistencia { InscripcionId = registro.InscripcionId, Fecha = fecha, Estado = registro.Estado });
            }
            await context.SaveChangesAsync();
        }

        private static IQueryable<AsistenciaTableDto> Proyectar(IQueryable<Asistencia> query)
        {
            return query.Select(a => new AsistenciaTableDto
            {
                Id = a.Id,
                Fecha = a.Fecha,
                Estado = a.Estado,
                InscripcionId = a.InscripcionId,
                EstudianteNombre = a.Inscripcion.Estudiante.Nombre + " " + a.Inscripcion.Estudiante.Apellido,
                Carnet = a.Inscripcion.Estudiante.Carnet,
                DeporteNombre = a.Inscripcion.Deporte.Nombre,
                PeriodoNombre = a.Inscripcion.Periodo.Nombre
            });
        }
    }
}

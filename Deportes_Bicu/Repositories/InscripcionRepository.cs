using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class InscripcionRepository(AppDbContext context) : IInscripcionRepository
    {
        public async Task<bool> CambiarEstadoAsync(int id, string estado)
        {
            var inscripcion = await context.Inscripcion.FindAsync(id);
            if (inscripcion == null) return false;

            inscripcion.Estado = estado;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarActivasAsync(int estudianteId, int periodoId)
        {
            return await context.Inscripcion.CountAsync(i => i.EstudianteId == estudianteId
                && i.PeriodoId == periodoId && i.Estado == Estados.InscripcionActiva);
        }

        public async Task<int> ContarInscritosAsync(int horarioId, int periodoId)
        {
            return await context.InscripcionHorario.CountAsync(ih => ih.HorarioId == horarioId
                && ih.Inscripcion.PeriodoId == periodoId
                && ih.Inscripcion.Estado == Estados.InscripcionActiva);
        }

        public async Task<Inscripcion> CreateAsync(Inscripcion inscripcion, IEnumerable<int> horarioIds)
        {
            inscripcion.InscripcionHorario = horarioIds
                .Select(id => new InscripcionHorario { HorarioId = id })
                .ToList();
            var entry = await context.Inscripcion.AddAsync(inscripcion);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var inscripcion = await context.Inscripcion.FindAsync(id);
            if (inscripcion == null) return false;

        
            context.Inscripcion.Remove(inscripcion);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeporteActivoAsync(int deporteId)
        {
            return await context.Deporte.AnyAsync(d => d.Id == deporteId && d.Estado == Estados.Activo);
        }

        public async Task<bool> EstudianteExisteAsync(int estudianteId)
        {
            return await context.Estudiante.AnyAsync(e => e.Id == estudianteId);
        }

        public async Task<bool> ExisteAsync(int estudianteId, int deporteId, int periodoId)
        {
            return await context.Inscripcion.AnyAsync(i => i.EstudianteId == estudianteId
                && i.DeporteId == deporteId && i.PeriodoId == periodoId);
        }

        public async Task<Inscripcion?> GetByIdAsync(int id)
        {
            return await context.Inscripcion.FindAsync(id);
        }

        public async Task<IReadOnlyList<OpcionDto>> GetCategoriasAsync(int deporteId)
        {
            return await context.DeporteCategoria
                .Where(dc => dc.DeporteId == deporteId)
                .OrderBy(dc => dc.CategoriaDeportiva.Nombre)
                .Select(dc => new OpcionDto { Id = dc.CategoriaDeportivaId, Texto = dc.CategoriaDeportiva.Nombre })
                .ToListAsync();
        }

        public async Task<int?> GetDeporteCategoriaIdAsync(int deporteId, int categoriaId)
        {
            return await context.DeporteCategoria
                .Where(dc => dc.DeporteId == deporteId && dc.CategoriaDeportivaId == categoriaId)
                .Select(dc => (int?)dc.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync()
        {
            return await context.Estudiante
                .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre)
                .Select(e => new OpcionDto { Id = e.Id, Texto = e.Apellido + ", " + e.Nombre + " (" + e.Carnet + ")" })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<HorarioDisponibleDto>> GetHorariosDisponiblesAsync(int deporteId, int periodoId)
        {
            var lista = await context.Horario
                .Where(h => h.EntrenadorDeporte.DeporteId == deporteId && h.Instalacion.Estado == Estados.Activo)
                .Select(h => new HorarioDisponibleDto
                {
                    Id = h.Id,
                    Dia = h.Dia,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    InstalacionNombre = h.Instalacion.Nombre,
                    EntrenadorNombre = h.EntrenadorDeporte.Entrenador.Nombre + " " + h.EntrenadorDeporte.Entrenador.Apellido,
                    Capacidad = h.Instalacion.Capacidad,
                    Inscritos = h.InscripcionHorario.Count(ih => ih.Inscripcion.PeriodoId == periodoId
                        && ih.Inscripcion.Estado == Estados.InscripcionActiva)
                }).ToListAsync();


            return lista
                .OrderBy(h => Array.IndexOf(Estados.Dias, h.Dia))
                .ThenBy(h => h.HoraInicio)
                .ToList();
        }

        public async Task<IReadOnlyList<HorarioInfoDto>> GetHorariosInfoAsync(IEnumerable<int> horarioIds)
        {
            var ids = horarioIds.ToList();
            return await context.Horario
                .Where(h => ids.Contains(h.Id))
                .Select(h => new HorarioInfoDto
                {
                    Id = h.Id,
                    Dia = h.Dia,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin,
                    DeporteId = h.EntrenadorDeporte.DeporteId,
                    DeporteNombre = h.EntrenadorDeporte.Deporte.Nombre,
                    InstalacionNombre = h.Instalacion.Nombre,
                    InstalacionActiva = h.Instalacion.Estado == Estados.Activo,
                    Capacidad = h.Instalacion.Capacidad
                }).ToListAsync();
        }

        public async Task<IReadOnlyList<HorarioInfoDto>> GetHorariosActivosAsync(int estudianteId, int periodoId)
        {
            return await context.InscripcionHorario
                .Where(ih => ih.Inscripcion.EstudianteId == estudianteId
                    && ih.Inscripcion.PeriodoId == periodoId
                    && ih.Inscripcion.Estado == Estados.InscripcionActiva)
                .Select(ih => new HorarioInfoDto
                {
                    Id = ih.HorarioId,
                    Dia = ih.Horario.Dia,
                    HoraInicio = ih.Horario.HoraInicio,
                    HoraFin = ih.Horario.HoraFin,
                    DeporteId = ih.Inscripcion.DeporteId,
                    DeporteNombre = ih.Inscripcion.Deporte.Nombre,
                    InstalacionNombre = ih.Horario.Instalacion.Nombre,
                    InstalacionActiva = ih.Horario.Instalacion.Estado == Estados.Activo,
                    Capacidad = ih.Horario.Instalacion.Capacidad
                }).ToListAsync();
        }

        public async Task<IReadOnlyList<InscripcionTableDto>> GetByEstudianteAsync(int estudianteId)
        {
            return await Proyectar(context.Inscripcion.Where(i => i.EstudianteId == estudianteId))
                .OrderByDescending(i => i.Fecha).ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<DataTableResponseDto<InscripcionTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Inscripcion.AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Estudiante.Nombre.Contains(search)
                || i.Estudiante.Apellido.Contains(search) || i.Estudiante.Carnet.Contains(search)
                || i.Deporte.Nombre.Contains(search) || i.Periodo.Nombre.Contains(search)
                || i.Estado.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "desc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "fecha";

            query = columnName.ToLowerInvariant() switch
            {
                "estudiantenombre" => orderDir == "desc" ? query.OrderByDescending(i => i.Estudiante.Apellido) : query.OrderBy(i => i.Estudiante.Apellido),
                "deportenombre" => orderDir == "desc" ? query.OrderByDescending(i => i.Deporte.Nombre) : query.OrderBy(i => i.Deporte.Nombre),
                "categorianombre" => orderDir == "desc" ? query.OrderByDescending(i => i.DeporteCategoria.CategoriaDeportiva.Nombre) : query.OrderBy(i => i.DeporteCategoria.CategoriaDeportiva.Nombre),
                "periodonombre" => orderDir == "desc" ? query.OrderByDescending(i => i.Periodo.Nombre) : query.OrderBy(i => i.Periodo.Nombre),
                "estado" => orderDir == "desc" ? query.OrderByDescending(i => i.Estado) : query.OrderBy(i => i.Estado),
                "id" => orderDir == "desc" ? query.OrderByDescending(i => i.Id) : query.OrderBy(i => i.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(i => i.Fecha).ThenByDescending(i => i.Id) : query.OrderBy(i => i.Fecha).ThenBy(i => i.Id),
            };

            var data = await Proyectar(query)
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .ToListAsync();

            return new DataTableResponseDto<InscripcionTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<Periodo?> GetPeriodoAsync(int periodoId)
        {
            return await context.Periodo.FindAsync(periodoId);
        }

        public async Task<bool> TieneAsistenciasAsync(int id)
        {
            return await context.Asistencia.AnyAsync(a => a.InscripcionId == id);
        }

  
        private static IQueryable<InscripcionTableDto> Proyectar(IQueryable<Inscripcion> query)
        {
            return query.Select(i => new InscripcionTableDto
            {
                Id = i.Id,
                Fecha = i.Fecha,
                Estado = i.Estado,
                EstudianteId = i.EstudianteId,
                EstudianteNombre = i.Estudiante.Nombre + " " + i.Estudiante.Apellido,
                Carnet = i.Estudiante.Carnet,
                DeporteNombre = i.Deporte.Nombre,
                CategoriaNombre = i.DeporteCategoria.CategoriaDeportiva.Nombre,
                PeriodoNombre = i.Periodo.Nombre,
                TotalAsistencias = i.Asistencia.Count,
                Horarios = i.InscripcionHorario.Select(ih => new HorarioResumenDto
                {
                    Dia = ih.Horario.Dia,
                    HoraInicio = ih.Horario.HoraInicio,
                    HoraFin = ih.Horario.HoraFin,
                    InstalacionNombre = ih.Horario.Instalacion.Nombre,
                    EntrenadorNombre = ih.Horario.EntrenadorDeporte.Entrenador.Nombre + " " + ih.Horario.EntrenadorDeporte.Entrenador.Apellido
                }).ToList()
            });
        }
    }
}

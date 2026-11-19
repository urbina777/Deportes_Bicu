using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class AsistenciaService(IAsistenciaRepository repo) : IAsistenciaService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveAsistenciaDto dto)
        {
            if (!Estados.Asistencia.Contains(dto.Estado))
                return (false, "Estado de asistencia no valido");

            var inscripcion = await repo.GetInscripcionInfoAsync(dto.InscripcionId);
            if (inscripcion == null)
                return (false, "La inscripcion seleccionada no existe");
            if (inscripcion.Estado != Estados.InscripcionActiva)
                return (false, "Solo se puede registrar asistencia en inscripciones activas");

            var error = ValidarFecha(inscripcion, dto.Fecha);
            if (error != null)
                return (false, error);

            if (await repo.ExisteAsync(dto.InscripcionId, dto.Fecha))
                return (false, "Ya existe una asistencia registrada para esa inscripcion en esa fecha");

            await repo.CreateAsync(new Asistencia
            {
                InscripcionId = dto.InscripcionId,
                Fecha = dto.Fecha,
                Estado = dto.Estado,
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Asistencia no encontrada");
        }

        public async Task<SaveAsistenciaDto?> GetByIdAsync(int id)
        {
            var asistencia = await repo.GetByIdAsync(id);
            if (asistencia == null)
                return null;
            return new SaveAsistenciaDto
            {
                Id = asistencia.Id,
                InscripcionId = asistencia.InscripcionId,
                Fecha = asistencia.Fecha,
                Estado = asistencia.Estado,
            };
        }

        public Task<IReadOnlyList<AsistenciaTableDto>> GetByEstudianteAsync(int estudianteId)
        => repo.GetByEstudianteAsync(estudianteId);

        public Task<IReadOnlyList<OpcionDto>> GetInscripcionesActivasOpcionesAsync()
        => repo.GetInscripcionesActivasOpcionesAsync();

        public Task<DataTableResponseDto<AsistenciaTableDto>> GetPagedAsync(DataTableRequestDto request, int? entrenadorId = null)
        => repo.GetPagedAsync(request, entrenadorId);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveAsistenciaDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var asistencia = await repo.GetByIdAsync(dto.Id.Value);
            if (asistencia == null)
                return (false, "Asistencia no encontrada");

            if (!Estados.Asistencia.Contains(dto.Estado))
                return (false, "Estado de asistencia no valido");

          
            var inscripcion = await repo.GetInscripcionInfoAsync(asistencia.InscripcionId);
            if (inscripcion == null)
                return (false, "La inscripcion de la asistencia no existe");

            var error = ValidarFecha(inscripcion, dto.Fecha);
            if (error != null)
                return (false, error);

            if (await repo.ExisteAsync(asistencia.InscripcionId, dto.Fecha, asistencia.Id))
                return (false, "Ya existe una asistencia registrada para esa inscripcion en esa fecha");

            asistencia.Fecha = dto.Fecha;
            asistencia.Estado = dto.Estado;

            await repo.UpdateAsync(asistencia);
            return (true, null);
        }

        public Task<IReadOnlyList<HorarioEntrenadorDto>> GetMisHorariosAsync(int entrenadorId)
        => repo.GetHorariosDeEntrenadorAsync(entrenadorId);

        public async Task<(IReadOnlyList<AsistenciaInscritoDto>? Data, string? ErrorMessage)> GetInscritosAsync(int entrenadorId, int horarioId, DateOnly fecha)
        {
            var (horario, error) = await ValidarSesionAsync(entrenadorId, horarioId, fecha);
            if (horario == null)
                return (null, error);

            return (await repo.GetInscritosDeHorarioAsync(horarioId, fecha), null);
        }

        public async Task<(bool Success, string? ErrorMessage)> GuardarSesionAsync(int entrenadorId, GuardarSesionDto dto)
        {
            var (horario, error) = await ValidarSesionAsync(entrenadorId, dto.HorarioId, dto.Fecha);
            if (horario == null)
                return (false, error);

            if (dto.Registros.Count == 0)
                return (false, "No hay asistencias para guardar");
            if (dto.Registros.Any(r => !Estados.Asistencia.Contains(r.Estado)))
                return (false, "Estado de asistencia no valido");
            if (dto.Registros.Select(r => r.InscripcionId).Distinct().Count() != dto.Registros.Count)
                return (false, "Hay estudiantes repetidos en la lista");


            var inscritos = await repo.GetInscritosDeHorarioAsync(dto.HorarioId, dto.Fecha);
            var permitidos = inscritos.Select(i => i.InscripcionId).ToHashSet();
            if (dto.Registros.Any(r => !permitidos.Contains(r.InscripcionId)))
                return (false, "Alguno de los estudiantes no esta inscrito en este horario");

            await repo.UpsertSesionAsync(dto.Fecha, dto.Registros);
            return (true, null);
        }

        
        private async Task<(HorarioEntrenadorDto? Horario, string? Error)> ValidarSesionAsync(int entrenadorId, int horarioId, DateOnly fecha)
        {
            var horario = await repo.GetHorarioDeEntrenadorAsync(horarioId, entrenadorId);
            if (horario == null)
                return (null, "Horario no encontrado");
            if (fecha > Hoy())
                return (null, "La fecha no puede ser futura");
            if (Estados.DiaDeSemana(fecha) != horario.Dia)
                return (null, $"La fecha seleccionada no es un {horario.Dia}");
            return (horario, null);
        }

     
        private static string? ValidarFecha(InscripcionAsistenciaInfoDto inscripcion, DateOnly fecha)
        {
            if (fecha > Hoy())
                return "La fecha no puede ser futura";
            if (fecha < inscripcion.FechaInicio || fecha > inscripcion.FechaFin)
                return "La fecha esta fuera del periodo de la inscripcion";
            if (!inscripcion.Dias.Contains(Estados.DiaDeSemana(fecha)))
                return "La fecha no corresponde a un dia de entrenamiento de la inscripcion";
            return null;
        }

        private static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.Now);
    }
}

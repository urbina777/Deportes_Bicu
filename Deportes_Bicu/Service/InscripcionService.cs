using Microsoft.Extensions.Options;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Models.Settings;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class InscripcionService(IInscripcionRepository repo, IOptions<ReglasInscripcionSettings> reglasOptions) : IInscripcionService
    {
        private readonly ReglasInscripcionSettings _reglas = reglasOptions.Value;

        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveInscripcionDto dto)
        {
            if (!await repo.EstudianteExisteAsync(dto.EstudianteId))
                return (false, "El estudiante seleccionado no existe");

            if (!await repo.DeporteActivoAsync(dto.DeporteId))
                return (false, "El deporte seleccionado no existe o esta inactivo");

            var periodo = await repo.GetPeriodoAsync(dto.PeriodoId);
            if (periodo == null)
                return (false, "El periodo seleccionado no existe");
            if (periodo.Estado != Estados.PeriodoAbierto)
                return (false, "El periodo seleccionado no esta abierto para inscripciones");

            var deporteCategoriaId = await repo.GetDeporteCategoriaIdAsync(dto.DeporteId, dto.CategoriaDeportivaId);
            if (deporteCategoriaId == null)
                return (false, "La categoria seleccionada no pertenece al deporte");

            var horarioIds = dto.HorarioIds.Distinct().ToList();
            if (horarioIds.Count == 0)
                return (false, "Seleccione al menos un horario");
            if (_reglas.MaxHorariosPorInscripcion > 0 && horarioIds.Count > _reglas.MaxHorariosPorInscripcion)
                return (false, $"Solo se permiten {_reglas.MaxHorariosPorInscripcion} horarios por inscripcion");

            var horarios = await repo.GetHorariosInfoAsync(horarioIds);
            if (horarios.Count != horarioIds.Count)
                return (false, "Alguno de los horarios seleccionados no existe");
            foreach (var h in horarios)
            {
                if (h.DeporteId != dto.DeporteId)
                    return (false, $"El horario {Describir(h)} no pertenece al deporte seleccionado");
                if (!h.InstalacionActiva)
                    return (false, $"La instalacion del horario {Describir(h)} esta inactiva");
            }

            if (await repo.ExisteAsync(dto.EstudianteId, dto.DeporteId, dto.PeriodoId))
                return (false, "El estudiante ya tiene una inscripcion en ese deporte y periodo");

            if (_reglas.MaxDeportesPorPeriodo > 0
                && await repo.ContarActivasAsync(dto.EstudianteId, dto.PeriodoId) >= _reglas.MaxDeportesPorPeriodo)
                return (false, $"Solo se permiten {_reglas.MaxDeportesPorPeriodo} deportes activos por periodo");

            
            for (var i = 0; i < horarios.Count; i++)
                for (var j = i + 1; j < horarios.Count; j++)
                    if (Choca(horarios[i], horarios[j]))
                        return (false, $"Los horarios {Describir(horarios[i])} y {Describir(horarios[j])} se cruzan");

         
            var existentes = await repo.GetHorariosActivosAsync(dto.EstudianteId, dto.PeriodoId);
            foreach (var h in horarios)
            {
                var choque = existentes.FirstOrDefault(e => Choca(h, e));
                if (choque != null)
                    return (false, $"El horario {Describir(h)} se cruza con {choque.DeporteNombre} ({Describir(choque)})");
            }

           
            foreach (var h in horarios)
            {
                var inscritos = await repo.ContarInscritosAsync(h.Id, dto.PeriodoId);
                if (inscritos >= h.Capacidad)
                    return (false, $"El horario {Describir(h)} no tiene cupo disponible");
            }

            await repo.CreateAsync(new Inscripcion
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now),
                Estado = Estados.InscripcionActiva,
                EstudianteId = dto.EstudianteId,
                DeporteId = dto.DeporteId,
                PeriodoId = dto.PeriodoId,
                DeporteCategoriaId = deporteCategoriaId.Value,
            }, horarioIds);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> CambiarEstadoAsync(int id, string estado, int? estudianteId = null)
        {
            if (estado != Estados.InscripcionFinalizada && estado != Estados.InscripcionCancelada)
                return (false, "Estado no valido");

            var inscripcion = await repo.GetByIdAsync(id);
      
            if (inscripcion == null || (estudianteId.HasValue && inscripcion.EstudianteId != estudianteId.Value))
                return (false, "Inscripcion no encontrada");

            if (estudianteId.HasValue && estado != Estados.InscripcionCancelada)
                return (false, "Solo puede cancelar su inscripcion");

            if (inscripcion.Estado != Estados.InscripcionActiva)
                return (false, "Solo se puede cambiar el estado de una inscripcion activa");

            await repo.CambiarEstadoAsync(id, estado);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneAsistenciasAsync(id))
                return (false, "La inscripcion tiene asistencias registradas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Inscripcion no encontrada");
        }

        public Task<IReadOnlyList<InscripcionTableDto>> GetByEstudianteAsync(int estudianteId)
        => repo.GetByEstudianteAsync(estudianteId);

        public Task<IReadOnlyList<OpcionDto>> GetCategoriasAsync(int deporteId)
        => repo.GetCategoriasAsync(deporteId);

        public Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync()
        => repo.GetEstudiantesOpcionesAsync();

        public Task<IReadOnlyList<HorarioDisponibleDto>> GetHorariosDisponiblesAsync(int deporteId, int periodoId)
        => repo.GetHorariosDisponiblesAsync(deporteId, periodoId);

        public Task<DataTableResponseDto<InscripcionTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        private static bool Choca(HorarioInfoDto a, HorarioInfoDto b)
        => a.Id != b.Id && Estados.Solapan(a.Dia, a.HoraInicio, a.HoraFin, b.Dia, b.HoraInicio, b.HoraFin);

        private static string Describir(HorarioInfoDto h)
        => $"{h.Dia} {h.HoraInicio:HH:mm}-{h.HoraFin:HH:mm}";
    }
}

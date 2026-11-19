using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class HorarioService(IHorarioRepository repo) : IHorarioService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveHorarioDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Horario
            {
                EntrenadorDeporteId = dto.EntrenadorDeporteId,
                InstalacionId = dto.InstalacionId,
                Dia = dto.Dia,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneInscripcionesAsync(id))
                return (false, "El horario tiene inscripciones registradas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Horario no encontrado");
        }

        public async Task<SaveHorarioDto?> GetByIdAsync(int id)
        {
            var horario = await repo.GetByIdAsync(id);
            if (horario == null)
                return null;
            return new SaveHorarioDto
            {
                Id = horario.Id,
                EntrenadorDeporteId = horario.EntrenadorDeporteId,
                InstalacionId = horario.InstalacionId,
                Dia = horario.Dia,
                HoraInicio = horario.HoraInicio,
                HoraFin = horario.HoraFin,
            };
        }

        public Task<IReadOnlyList<OpcionDto>> GetAsignacionesOpcionesAsync()
        => repo.GetAsignacionesOpcionesAsync();

        public Task<DataTableResponseDto<HorarioTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveHorarioDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var horario = await repo.GetByIdAsync(dto.Id.Value);
            if (horario == null)
                return (false, "Horario no encontrado");

            var cambio = horario.Dia != dto.Dia || horario.HoraInicio != dto.HoraInicio || horario.HoraFin != dto.HoraFin
                || horario.InstalacionId != dto.InstalacionId || horario.EntrenadorDeporteId != dto.EntrenadorDeporteId;
            if (cambio && await repo.TieneInscripcionesAsync(horario.Id))
                return (false, "El horario tiene inscripciones registradas y no puede modificarse");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            horario.EntrenadorDeporteId = dto.EntrenadorDeporteId;
            horario.InstalacionId = dto.InstalacionId;
            horario.Dia = dto.Dia;
            horario.HoraInicio = dto.HoraInicio;
            horario.HoraFin = dto.HoraFin;

            await repo.UpdateAsync(horario);
            return (true, null);
        }

        private async Task<string?> ValidarAsync(SaveHorarioDto dto, int? excludeId)
        {
            if (!Estados.Dias.Contains(dto.Dia))
                return "Dia no valido";

            if (dto.HoraFin <= dto.HoraInicio)
                return "La hora de fin debe ser posterior a la hora de inicio";

            var entrenadorId = await repo.GetEntrenadorIdAsync(dto.EntrenadorDeporteId);
            if (entrenadorId == null)
                return "La asignacion entrenador - deporte seleccionada no es valida";

            if (!await repo.InstalacionActivaAsync(dto.InstalacionId))
                return "La instalacion seleccionada no existe o esta inactiva";

            if (await repo.ChoqueEntrenadorAsync(dto.Dia, dto.HoraInicio, dto.HoraFin, entrenadorId.Value, excludeId))
                return "El entrenador ya tiene otro horario que se cruza con ese dia y hora";

            if (await repo.ChoqueInstalacionAsync(dto.Dia, dto.HoraInicio, dto.HoraFin, dto.InstalacionId, excludeId))
                return "La instalacion ya esta ocupada en ese dia y hora";

            return null;
        }
    }
}

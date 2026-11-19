using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class PeriodoService(IPeriodoRepository repo) : IPeriodoService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SavePeriodoDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Periodo
            {
                Nombre = dto.Nombre.Trim(),
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Estado = dto.Estado
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneRelacionadosAsync(id))
                return (false, "El período tiene inscripciones registradas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Período no encontrado");
        }

        public async Task<SavePeriodoDto?> GetByIdAsync(int id)
        {
            var entidad = await repo.GetByIdAsync(id);
            if (entidad == null)
                return null;
            return new SavePeriodoDto
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                FechaInicio = entidad.FechaInicio,
                FechaFin = entidad.FechaFin,
                Estado = entidad.Estado
            };
        }

        public Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        => repo.GetOpcionesAsync(soloDisponibles);

        public Task<DataTableResponseDto<PeriodoTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SavePeriodoDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var entidad = await repo.GetByIdAsync(dto.Id.Value);
            if (entidad == null)
                return (false, "Período no encontrado");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            entidad.Nombre = dto.Nombre.Trim();
            entidad.FechaInicio = dto.FechaInicio;
            entidad.FechaFin = dto.FechaFin;
            entidad.Estado = dto.Estado;
            await repo.UpdateAsync(entidad);
            return (true, null);
        }

      
        private async Task<string?> ValidarAsync(SavePeriodoDto dto, int? excludeId)
        {
            if (await repo.NombreExisteAsync(dto.Nombre.Trim(), excludeId))
                return "Ya existe un período con ese nombre";
            if (dto.FechaInicio == default)
                return "La fecha de inicio es obligatoria";
            if (dto.FechaFin == default)
                return "La fecha de fin es obligatoria";
            if (!Estados.Periodo.Contains(dto.Estado))
                return "Estado no valido";
            if (dto.FechaFin < dto.FechaInicio)
                return "La fecha de fin no puede ser anterior a la fecha de inicio";
            return null;
        }

    }
}

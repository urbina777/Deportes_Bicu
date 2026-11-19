using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class InstalacionService(IInstalacionRepository repo) : IInstalacionService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveInstalacionDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Instalacion
            {
                Nombre = dto.Nombre.Trim(),
                Ubicacion = string.IsNullOrWhiteSpace(dto.Ubicacion) ? null : dto.Ubicacion.Trim(),
                Capacidad = dto.Capacidad,
                Estado = dto.Estado
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneRelacionadosAsync(id))
                return (false, "La instalación tiene horarios asignados");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Instalación no encontrada");
        }

        public async Task<SaveInstalacionDto?> GetByIdAsync(int id)
        {
            var entidad = await repo.GetByIdAsync(id);
            if (entidad == null)
                return null;
            return new SaveInstalacionDto
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                Ubicacion = entidad.Ubicacion,
                Capacidad = entidad.Capacidad,
                Estado = entidad.Estado
            };
        }

        public Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        => repo.GetOpcionesAsync(soloDisponibles);

        public Task<DataTableResponseDto<InstalacionTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveInstalacionDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var entidad = await repo.GetByIdAsync(dto.Id.Value);
            if (entidad == null)
                return (false, "Instalación no encontrada");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            entidad.Nombre = dto.Nombre.Trim();
            entidad.Ubicacion = string.IsNullOrWhiteSpace(dto.Ubicacion) ? null : dto.Ubicacion.Trim();
            entidad.Capacidad = dto.Capacidad;
            entidad.Estado = dto.Estado;
            await repo.UpdateAsync(entidad);
            return (true, null);
        }

      
        private async Task<string?> ValidarAsync(SaveInstalacionDto dto, int? excludeId)
        {
            if (await repo.NombreExisteAsync(dto.Nombre.Trim(), excludeId))
                return "Ya existe una instalación con ese nombre";
            if (!Estados.Instalacion.Contains(dto.Estado))
                return "Estado no valido";
            if (excludeId.HasValue && await repo.MaxOcupacionAsync(excludeId.Value) > dto.Capacidad)
                return "La capacidad no puede ser menor que los cupos ya ocupados";
            return null;
        }

    }
}

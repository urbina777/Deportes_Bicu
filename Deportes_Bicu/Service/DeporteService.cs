using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class DeporteService(IDeporteRepository repo) : IDeporteService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveDeporteDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Deporte
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
                Estado = dto.Estado
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneRelacionadosAsync(id))
                return (false, "El deporte tiene inscripciones, entrenadores o categorías asignadas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Deporte no encontrado");
        }

        public async Task<SaveDeporteDto?> GetByIdAsync(int id)
        {
            var entidad = await repo.GetByIdAsync(id);
            if (entidad == null)
                return null;
            return new SaveDeporteDto
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                Descripcion = entidad.Descripcion,
                Estado = entidad.Estado
            };
        }

        public Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        => repo.GetOpcionesAsync(soloDisponibles);

        public Task<DataTableResponseDto<DeporteTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveDeporteDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var entidad = await repo.GetByIdAsync(dto.Id.Value);
            if (entidad == null)
                return (false, "Deporte no encontrado");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            entidad.Nombre = dto.Nombre.Trim();
            entidad.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            entidad.Estado = dto.Estado;
            await repo.UpdateAsync(entidad);
            return (true, null);
        }

        
        private async Task<string?> ValidarAsync(SaveDeporteDto dto, int? excludeId)
        {
            if (await repo.NombreExisteAsync(dto.Nombre.Trim(), excludeId))
                return "Ya existe un deporte con ese nombre";
            if (!Estados.Deporte.Contains(dto.Estado))
                return "Estado no valido";
            return null;
        }

        public Task<AsignacionesDto> GetCategoriasAsync(int deporteId)
        => repo.GetCategoriasAsync(deporteId);

        public async Task<(bool Success, string? ErrorMessage)> AsignarCategoriaAsync(int deporteId, int categoriaId)
        {
            if (await repo.GetByIdAsync(deporteId) == null)
                return (false, "Deporte no encontrado");
            if (!await repo.CategoriaExisteAsync(categoriaId))
                return (false, "Categoria seleccionada no valida");
            if (await repo.CategoriaAsignadaAsync(deporteId, categoriaId))
                return (false, "El deporte ya tiene asignada esa categoria");

            await repo.AsignarCategoriaAsync(deporteId, categoriaId);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> QuitarCategoriaAsync(int deporteCategoriaId)
        {
            if (await repo.CategoriaTieneInscripcionesAsync(deporteCategoriaId))
                return (false, "No se puede quitar la categoria: tiene inscripciones registradas");
            var removed = await repo.QuitarCategoriaAsync(deporteCategoriaId);
            return removed ? (true, null) : (false, "Asignacion no encontrada");
        }
    }
}

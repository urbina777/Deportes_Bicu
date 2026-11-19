using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class CategoriaDeportivaService(ICategoriaDeportivaRepository repo) : ICategoriaDeportivaService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveCategoriaDeportivaDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new CategoriaDeportiva
            {
                Nombre = dto.Nombre.Trim(),
                EdadMinima = dto.EdadMinima,
                EdadMaxima = dto.EdadMaxima
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.TieneRelacionadosAsync(id))
                return (false, "La categoría está asignada a uno o más deportes");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Categoría deportiva no encontrada");
        }

        public async Task<SaveCategoriaDeportivaDto?> GetByIdAsync(int id)
        {
            var entidad = await repo.GetByIdAsync(id);
            if (entidad == null)
                return null;
            return new SaveCategoriaDeportivaDto
            {
                Id = entidad.Id,
                Nombre = entidad.Nombre,
                EdadMinima = entidad.EdadMinima,
                EdadMaxima = entidad.EdadMaxima
            };
        }

        public Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false)
        => repo.GetOpcionesAsync(soloDisponibles);

        public Task<DataTableResponseDto<CategoriaDeportivaTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveCategoriaDeportivaDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var entidad = await repo.GetByIdAsync(dto.Id.Value);
            if (entidad == null)
                return (false, "Categoría deportiva no encontrada");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            entidad.Nombre = dto.Nombre.Trim();
            entidad.EdadMinima = dto.EdadMinima;
            entidad.EdadMaxima = dto.EdadMaxima;
            await repo.UpdateAsync(entidad);
            return (true, null);
        }

      
        private async Task<string?> ValidarAsync(SaveCategoriaDeportivaDto dto, int? excludeId)
        {
            if (await repo.NombreExisteAsync(dto.Nombre.Trim(), excludeId))
                return "Ya existe una categoría deportiva con ese nombre";
            if (dto.EdadMaxima < dto.EdadMinima)
                return "La edad maxima no puede ser menor que la edad minima";
            return null;
        }

    }
}

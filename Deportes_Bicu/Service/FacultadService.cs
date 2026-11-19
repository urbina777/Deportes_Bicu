using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class FacultadService(IFacultadRepository repo) : IFacultadService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveFacultadDto dto)
        {
            var nombre = dto.Nombre.Trim();
            if (await repo.NombreExisteAsync(nombre))
                return (false, "Ya existe una facultad con ese nombre");
            await repo.CreateAsync(new Facultad
            {
                Nombre = nombre,
                Descripcion = dto.Descripcion?.Trim(),
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.HasAnyCarreraAsync(id))
                return (false, "La facultad tiene carreras asignadas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Facultad no encontrada");
        }

        public async Task<IReadOnlyList<FacultadTableDto>> GetAllAsync()
        {
            var facultad = await repo.GetAllAsync();
            return facultad.Select(x => new FacultadTableDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Descripcion = x.Descripcion,
            }).ToList();
        }

        public async Task<SaveFacultadDto?> GetByIdAsync(int id)
        {
            var facultad = await repo.GetByIdAsync(id);
            if (facultad == null)
                return null;
            return new SaveFacultadDto
            {
                Id = facultad.Id,
                Nombre = facultad.Nombre,
                Descripcion = facultad.Descripcion,
            };
        }

        public Task<DataTableResponseDto<FacultadTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveFacultadDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var facultad = await repo.GetByIdAsync(dto.Id.Value);
            if (facultad == null)
                return (false, "Facultad no encontrada");

            var nombre = dto.Nombre.Trim();
            if (await repo.NombreExisteAsync(nombre, dto.Id))
                return (false, "Ya existe una facultad con ese nombre");

            facultad.Nombre = nombre;
            facultad.Descripcion = dto.Descripcion?.Trim();
            await repo.UpdateAsync(facultad);
            return (true, null);
        }
    }
}

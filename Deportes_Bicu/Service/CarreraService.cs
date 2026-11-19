using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class CarreraService(ICarreraRepository repo, IFacultadRepository repoFacultad) : ICarreraService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveCarreraDto dto)
        {
            if (await repoFacultad.GetByIdAsync(dto.FacultadId) == null)
                return (false, "Facultad seleccionada no valida");

            var nombre = dto.Nombre.Trim();
            if (await repo.NombreExisteAsync(nombre, dto.FacultadId))
                return (false, "Ya existe esa carrera en la facultad seleccionada");

            await repo.CreateAsync(new Carrera
            {
                Nombre = nombre,
                FacultadId = dto.FacultadId,
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.HasAnyEstudianteAsync(id))
                return (false, "La carrera tiene estudiantes asignados");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Carrera no encontrada");
        }

        public async Task<IReadOnlyList<CarreraTableDto>> GetAllAsync()
        {
            var carrera = await repo.GetAllAsync();
            return carrera.Select(x => new CarreraTableDto
            {
                Id = x.Id,
                Nombre = x.Nombre,
                FacultadId = x.FacultadId,
                FacultadNombre = x.Facultad.Nombre,
            }).ToList();
        }

        public async Task<SaveCarreraDto?> GetByIdAsync(int id)
        {
            var carrera = await repo.GetByIdAsync(id);
            if (carrera == null)
                return null;
            return new SaveCarreraDto
            {
                Id = carrera.Id,
                Nombre = carrera.Nombre,
                FacultadId = carrera.FacultadId,
            };
        }

        public Task<DataTableResponseDto<CarreraTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveCarreraDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var carrera = await repo.GetByIdAsync(dto.Id.Value);
            if (carrera == null)
                return (false, "Carrera no encontrada");
            if (await repoFacultad.GetByIdAsync(dto.FacultadId) == null)
                return (false, "Facultad seleccionada no valida");

            var nombre = dto.Nombre.Trim();
            if (await repo.NombreExisteAsync(nombre, dto.FacultadId, dto.Id))
                return (false, "Ya existe esa carrera en la facultad seleccionada");

            carrera.Nombre = nombre;
            carrera.FacultadId = dto.FacultadId;
            await repo.UpdateAsync(carrera);
            return (true, null);
        }
    }
}

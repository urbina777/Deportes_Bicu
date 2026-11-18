using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface ICarreraRepository
    {
        Task<DataTableResponseDto<CarreraTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<Carrera>> GetAllAsync();
        Task<Carrera?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int facultadId, int? excludeId = null);
        Task<Carrera> CreateAsync(Carrera carrera);
        Task<bool> UpdateAsync(Carrera carrera);
        Task<bool> DeleteAsync(int Id);
        Task<bool> HasAnyEstudianteAsync(int Id);
    }
}

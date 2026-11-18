using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IFacultadRepository
    {
        Task<DataTableResponseDto<FacultadTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<Facultad>> GetAllAsync();
        Task<Facultad?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
        Task<Facultad> CreateAsync(Facultad facultad);
        Task<bool> UpdateAsync(Facultad facultad);
        Task<bool> DeleteAsync(int Id);
        Task<bool> HasAnyCarreraAsync(int Id);
    }
}

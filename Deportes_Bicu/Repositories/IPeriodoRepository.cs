using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IPeriodoRepository
    {
        Task<DataTableResponseDto<PeriodoTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<Periodo?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
        Task<Periodo> CreateAsync(Periodo entidad);
        Task<bool> UpdateAsync(Periodo entidad);
        Task<bool> DeleteAsync(int Id);
        Task<bool> TieneRelacionadosAsync(int Id);

    }
}

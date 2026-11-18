using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IInstalacionRepository
    {
        Task<DataTableResponseDto<InstalacionTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<Instalacion?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
        Task<Instalacion> CreateAsync(Instalacion entidad);
        Task<bool> UpdateAsync(Instalacion entidad);
        Task<bool> DeleteAsync(int Id);
        Task<bool> TieneRelacionadosAsync(int Id);
        Task<int> MaxOcupacionAsync(int instalacionId);
    }
}

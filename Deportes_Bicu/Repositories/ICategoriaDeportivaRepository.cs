using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface ICategoriaDeportivaRepository
    {
        Task<DataTableResponseDto<CategoriaDeportivaTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<CategoriaDeportiva?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
        Task<CategoriaDeportiva> CreateAsync(CategoriaDeportiva entidad);
        Task<bool> UpdateAsync(CategoriaDeportiva entidad);
        Task<bool> DeleteAsync(int Id);
        Task<bool> TieneRelacionadosAsync(int Id);

    }
}

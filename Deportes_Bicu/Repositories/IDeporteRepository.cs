using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IDeporteRepository
    {
        Task<DataTableResponseDto<DeporteTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<Deporte?> GetByIdAsync(int Id);
        Task<bool> NombreExisteAsync(string nombre, int? excludeId = null);
        Task<Deporte> CreateAsync(Deporte entidad);
        Task<bool> UpdateAsync(Deporte entidad);
        Task<bool> DeleteAsync(int Id);
        Task<bool> TieneRelacionadosAsync(int Id);
        Task<AsignacionesDto> GetCategoriasAsync(int deporteId);
        Task<bool> CategoriaExisteAsync(int categoriaId);
        Task<bool> CategoriaAsignadaAsync(int deporteId, int categoriaId);
        Task AsignarCategoriaAsync(int deporteId, int categoriaId);
        Task<bool> CategoriaTieneInscripcionesAsync(int deporteCategoriaId);
        Task<bool> QuitarCategoriaAsync(int deporteCategoriaId);
    }
}

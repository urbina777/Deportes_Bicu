using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IDeporteService
    {
        Task<DataTableResponseDto<DeporteTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<SaveDeporteDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveDeporteDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveDeporteDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
        Task<AsignacionesDto> GetCategoriasAsync(int deporteId);
        Task<(bool Success, string? ErrorMessage)> AsignarCategoriaAsync(int deporteId, int categoriaId);
        Task<(bool Success, string? ErrorMessage)> QuitarCategoriaAsync(int deporteCategoriaId);
    }
}

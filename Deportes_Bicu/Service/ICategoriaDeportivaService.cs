using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface ICategoriaDeportivaService
    {
        Task<DataTableResponseDto<CategoriaDeportivaTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<SaveCategoriaDeportivaDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveCategoriaDeportivaDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveCategoriaDeportivaDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    }
}

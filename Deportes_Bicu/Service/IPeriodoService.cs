using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IPeriodoService
    {
        Task<DataTableResponseDto<PeriodoTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<SavePeriodoDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SavePeriodoDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SavePeriodoDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    }
}

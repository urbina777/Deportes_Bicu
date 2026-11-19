using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IInstalacionService
    {
        Task<DataTableResponseDto<InstalacionTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<OpcionDto>> GetOpcionesAsync(bool soloDisponibles = false);
        Task<SaveInstalacionDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveInstalacionDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveInstalacionDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

    }
}

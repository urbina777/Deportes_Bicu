using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface ICarreraService
    {
        Task<DataTableResponseDto<CarreraTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<CarreraTableDto>> GetAllAsync();
        Task<SaveCarreraDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveCarreraDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveCarreraDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

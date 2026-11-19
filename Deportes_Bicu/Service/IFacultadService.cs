using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IFacultadService
    {
        Task<DataTableResponseDto<FacultadTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<FacultadTableDto>> GetAllAsync();
        Task<SaveFacultadDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveFacultadDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveFacultadDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

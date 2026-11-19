using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IHorarioService
    {
        Task<DataTableResponseDto<HorarioTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<SaveHorarioDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OpcionDto>> GetAsignacionesOpcionesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveHorarioDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveHorarioDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

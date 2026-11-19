using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IEntrenadorService
    {
        Task<DataTableResponseDto<EntrenadorTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<SaveEntrenadorDto?> GetByIdAsync(int id);
        Task<int?> GetIdPorUsuarioAsync(int usuarioId);
        Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEntrenadorDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEntrenadorDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
        Task<AsignacionesDto> GetDeportesAsync(int entrenadorId);
        Task<(bool Success, string? ErrorMessage)> AsignarDeporteAsync(int entrenadorId, int deporteId);
        Task<(bool Success, string? ErrorMessage)> QuitarDeporteAsync(int entrenadorDeporteId);
    }
}

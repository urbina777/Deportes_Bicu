using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IInscripcionService
    {
        Task<DataTableResponseDto<InscripcionTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<InscripcionTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync();
        Task<IReadOnlyList<OpcionDto>> GetCategoriasAsync(int deporteId);
        Task<IReadOnlyList<HorarioDisponibleDto>> GetHorariosDisponiblesAsync(int deporteId, int periodoId);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveInscripcionDto dto);
      
        Task<(bool Success, string? ErrorMessage)> CambiarEstadoAsync(int id, string estado, int? estudianteId = null);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

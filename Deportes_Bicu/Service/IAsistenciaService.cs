using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IAsistenciaService
    {
        Task<DataTableResponseDto<AsistenciaTableDto>> GetPagedAsync(DataTableRequestDto request, int? entrenadorId = null);
        Task<IReadOnlyList<AsistenciaTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<SaveAsistenciaDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<OpcionDto>> GetInscripcionesActivasOpcionesAsync();
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveAsistenciaDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveAsistenciaDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);

        
        Task<IReadOnlyList<HorarioEntrenadorDto>> GetMisHorariosAsync(int entrenadorId);
        Task<(IReadOnlyList<AsistenciaInscritoDto>? Data, string? ErrorMessage)> GetInscritosAsync(int entrenadorId, int horarioId, DateOnly fecha);
        Task<(bool Success, string? ErrorMessage)> GuardarSesionAsync(int entrenadorId, GuardarSesionDto dto);
    }
}

using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IEvaluacionFisicaService
    {
        Task<DataTableResponseDto<EvaluacionFisicaTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<EvaluacionFisicaTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync();
        Task<SaveEvaluacionFisicaDto?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEvaluacionFisicaDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEvaluacionFisicaDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

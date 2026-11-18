using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IEvaluacionFisicaRepository
    {
        Task<DataTableResponseDto<EvaluacionFisicaTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<EvaluacionFisicaTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<EvaluacionFisica?> GetByIdAsync(int id);
        Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync();
        Task<bool> EstudianteExisteAsync(int estudianteId);
        Task<EvaluacionFisica> CreateAsync(EvaluacionFisica evaluacion);
        Task<bool> UpdateAsync(EvaluacionFisica evaluacion);
        Task<bool> DeleteAsync(int id);
    }
}

using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IInscripcionRepository
    {
        Task<DataTableResponseDto<InscripcionTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<IReadOnlyList<InscripcionTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<Inscripcion?> GetByIdAsync(int id);
        Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync();
        Task<bool> EstudianteExisteAsync(int estudianteId);
        Task<bool> DeporteActivoAsync(int deporteId);
        Task<Periodo?> GetPeriodoAsync(int periodoId);
       
        Task<int?> GetDeporteCategoriaIdAsync(int deporteId, int categoriaId);
        Task<IReadOnlyList<OpcionDto>> GetCategoriasAsync(int deporteId);
        Task<IReadOnlyList<HorarioDisponibleDto>> GetHorariosDisponiblesAsync(int deporteId, int periodoId);
        Task<IReadOnlyList<HorarioInfoDto>> GetHorariosInfoAsync(IEnumerable<int> horarioIds);
        Task<bool> ExisteAsync(int estudianteId, int deporteId, int periodoId);
        Task<int> ContarActivasAsync(int estudianteId, int periodoId);
       
        Task<IReadOnlyList<HorarioInfoDto>> GetHorariosActivosAsync(int estudianteId, int periodoId);
        Task<int> ContarInscritosAsync(int horarioId, int periodoId);
        Task<Inscripcion> CreateAsync(Inscripcion inscripcion, IEnumerable<int> horarioIds);
        Task<bool> CambiarEstadoAsync(int id, string estado);
        Task<bool> TieneAsistenciasAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

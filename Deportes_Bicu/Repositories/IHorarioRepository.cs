using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IHorarioRepository
    {
        Task<DataTableResponseDto<HorarioTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<Horario?> GetByIdAsync(int id);
        Task<IReadOnlyList<OpcionDto>> GetAsignacionesOpcionesAsync();
        Task<int?> GetEntrenadorIdAsync(int entrenadorDeporteId);
        Task<bool> InstalacionActivaAsync(int instalacionId);
        Task<bool> ChoqueInstalacionAsync(string dia, TimeOnly inicio, TimeOnly fin, int instalacionId, int? excludeId = null);
        Task<bool> ChoqueEntrenadorAsync(string dia, TimeOnly inicio, TimeOnly fin, int entrenadorId, int? excludeId = null);
        Task<bool> TieneInscripcionesAsync(int id);
        Task<Horario> CreateAsync(Horario horario);
        Task<bool> UpdateAsync(Horario horario);
        Task<bool> DeleteAsync(int id);
    }
}

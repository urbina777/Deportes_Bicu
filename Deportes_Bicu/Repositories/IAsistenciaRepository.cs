using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IAsistenciaRepository
    {
      
        Task<DataTableResponseDto<AsistenciaTableDto>> GetPagedAsync(DataTableRequestDto request, int? entrenadorId = null);
        Task<IReadOnlyList<AsistenciaTableDto>> GetByEstudianteAsync(int estudianteId);
        Task<Asistencia?> GetByIdAsync(int id);
        Task<InscripcionAsistenciaInfoDto?> GetInscripcionInfoAsync(int inscripcionId);
        Task<IReadOnlyList<OpcionDto>> GetInscripcionesActivasOpcionesAsync();
        Task<bool> ExisteAsync(int inscripcionId, DateOnly fecha, int? excludeId = null);
        Task<Asistencia> CreateAsync(Asistencia asistencia);
        Task<bool> UpdateAsync(Asistencia asistencia);
        Task<bool> DeleteAsync(int id);


        Task<IReadOnlyList<HorarioEntrenadorDto>> GetHorariosDeEntrenadorAsync(int entrenadorId);
        Task<HorarioEntrenadorDto?> GetHorarioDeEntrenadorAsync(int horarioId, int entrenadorId);
        Task<IReadOnlyList<AsistenciaInscritoDto>> GetInscritosDeHorarioAsync(int horarioId, DateOnly fecha);
        Task UpsertSesionAsync(DateOnly fecha, IEnumerable<RegistroAsistenciaDto> registros);
    }
}

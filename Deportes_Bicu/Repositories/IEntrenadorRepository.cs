using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IEntrenadorRepository
    {
        Task<DataTableResponseDto<EntrenadorTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<Entrenador?> GetByIdAsync(int id);
        Task<int?> GetIdPorUsuarioAsync(int usuarioId);
        Task<bool> CorreoExisteAsync(string correo, int? excludeId = null);

        Task<bool> CodigoExisteAsync(string codigo, int usuarioId, int? excludeId = null);
        Task<bool> UsuarioExisteAsync(int usuarioId);
        Task<bool> UsuarioTienePerfilAsync(int usuarioId, int? excludeId = null);
        Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null);
        Task<bool> HasRelatedRecordsAsync(int id);
        Task<Entrenador> CreateAsync(Entrenador entrenador);
        Task<bool> UpdateAsync(Entrenador entrenador);
        Task<bool> DeleteAsync(int Id);


        Task<AsignacionesDto> GetDeportesAsync(int entrenadorId);
        Task<bool> DeporteActivoAsync(int deporteId);
        Task<bool> DeporteAsignadoAsync(int entrenadorId, int deporteId);
        Task AsignarDeporteAsync(int entrenadorId, int deporteId);
        Task<bool> AsignacionTieneHorariosAsync(int entrenadorDeporteId);
        Task<bool> QuitarDeporteAsync(int entrenadorDeporteId);
    }
}

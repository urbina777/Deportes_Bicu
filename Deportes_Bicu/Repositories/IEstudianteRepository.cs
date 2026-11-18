using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public interface IEstudianteRepository
    {
        Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<Estudiante?> GetByIdAsync(int id);
        Task<int?> GetIdPorUsuarioAsync(int usuarioId);
        Task<EstudianteTableDto?> GetPerfilAsync(int id);
        Task<bool> CorreoExisteAsync(string correo, int? excludeId = null);
    
        Task<bool> CodigoExisteAsync(string codigo, int usuarioId, int? excludeId = null);
        Task<bool> UsuarioExisteAsync(int usuarioId);
        Task<bool> UsuarioTienePerfilAsync(int usuarioId, int? excludeId = null);
    
        Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null);
        Task<bool> HasRelatedRecordsAsync(int id);
        Task<Estudiante> CreateAsync(Estudiante estudiante);
        Task<bool> UpdateAsync(Estudiante estudiante);
        Task<bool> DeleteAsync(int Id);
    }
}

using Deportes_Bicu.DTOS;

namespace Deportes_Bicu.Service
{
    public interface IEstudianteService
    {
        Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(DataTableRequestDto request);
        Task<SaveEstudianteDto?> GetByIdAsync(int id);
        Task<int?> GetIdPorUsuarioAsync(int usuarioId);
        Task<EstudianteTableDto?> GetPerfilAsync(int id);
        Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEstudianteDto dto);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEstudianteDto dto);
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    }
}

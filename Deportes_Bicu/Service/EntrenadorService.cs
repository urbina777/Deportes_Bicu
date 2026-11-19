using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class EntrenadorService(IEntrenadorRepository repo) : IEntrenadorService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEntrenadorDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Entrenador
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Cedula = dto.Cedula.Trim(),
                Carnet = dto.Carnet.Trim(),
                Correo = dto.Correo.Trim(),
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                UsuarioId = dto.UsuarioId,
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.HasRelatedRecordsAsync(id))
                return (false, "El entrenador tiene deportes asignados");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Entrenador no encontrado");
        }

        public async Task<SaveEntrenadorDto?> GetByIdAsync(int id)
        {
            var entrenador = await repo.GetByIdAsync(id);
            if (entrenador == null)
                return null;
            return new SaveEntrenadorDto
            {
                Id = entrenador.Id,
                Nombre = entrenador.Nombre,
                Apellido = entrenador.Apellido,
                Cedula = entrenador.Cedula,
                Carnet = entrenador.Carnet,
                Correo = entrenador.Correo,
                Telefono = entrenador.Telefono,
                UsuarioId = entrenador.UsuarioId,
            };
        }

        public Task<int?> GetIdPorUsuarioAsync(int usuarioId)
        => repo.GetIdPorUsuarioAsync(usuarioId);

        public Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null)
        => repo.GetUsuariosDisponiblesAsync(incluirUsuarioId);

        public Task<DataTableResponseDto<EntrenadorTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEntrenadorDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var entrenador = await repo.GetByIdAsync(dto.Id.Value);
            if (entrenador == null)
                return (false, "Entrenador no encontrado");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            entrenador.Nombre = dto.Nombre.Trim();
            entrenador.Apellido = dto.Apellido.Trim();
            entrenador.Cedula = dto.Cedula.Trim();
            entrenador.Carnet = dto.Carnet.Trim();
            entrenador.Correo = dto.Correo.Trim();
            entrenador.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            entrenador.UsuarioId = dto.UsuarioId;

            await repo.UpdateAsync(entrenador);
            return (true, null);
        }

        public Task<AsignacionesDto> GetDeportesAsync(int entrenadorId)
        => repo.GetDeportesAsync(entrenadorId);

        public async Task<(bool Success, string? ErrorMessage)> AsignarDeporteAsync(int entrenadorId, int deporteId)
        {
            if (await repo.GetByIdAsync(entrenadorId) == null)
                return (false, "Entrenador no encontrado");
            if (!await repo.DeporteActivoAsync(deporteId))
                return (false, "Deporte seleccionado no valido o inactivo");
            if (await repo.DeporteAsignadoAsync(entrenadorId, deporteId))
                return (false, "El entrenador ya esta asignado a ese deporte");

            await repo.AsignarDeporteAsync(entrenadorId, deporteId);
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> QuitarDeporteAsync(int entrenadorDeporteId)
        {
            if (await repo.AsignacionTieneHorariosAsync(entrenadorDeporteId))
                return (false, "No se puede quitar el deporte: la asignacion tiene horarios registrados");
            var removed = await repo.QuitarDeporteAsync(entrenadorDeporteId);
            return removed ? (true, null) : (false, "Asignacion no encontrada");
        }

     
        private async Task<string?> ValidarAsync(SaveEntrenadorDto dto, int? excludeId)
        {
            if (!await repo.UsuarioExisteAsync(dto.UsuarioId))
                return "La cuenta de usuario seleccionada no existe";

            if (await repo.UsuarioTienePerfilAsync(dto.UsuarioId, excludeId))
                return "Esa cuenta de usuario ya tiene un perfil de entrenador";

            if (await repo.CorreoExisteAsync(dto.Correo.Trim(), excludeId))
                return "Ya existe un entrenador con ese correo";

            if (await repo.CodigoExisteAsync(dto.Carnet.Trim(), dto.UsuarioId, excludeId))
                return "El codigo institucional ya pertenece a otra persona";

            return null;
        }
    }
}

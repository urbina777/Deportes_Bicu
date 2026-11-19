using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class EstudianteService(IEstudianteRepository repo, ICarreraRepository repoCarrera) : IEstudianteService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEstudianteDto dto)
        {
            var error = await ValidarAsync(dto, null);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new Estudiante
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Cedula = dto.Cedula.Trim(),
                Carnet = dto.Carnet.Trim(),
                Correo = dto.Correo.Trim(),
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                CarreraId = dto.CarreraId,
                UsuarioId = dto.UsuarioId,
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            if (await repo.HasRelatedRecordsAsync(id))
                return (false, "El estudiante tiene inscripciones o evaluaciones fisicas registradas");
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Estudiante no encontrado");
        }

        public async Task<SaveEstudianteDto?> GetByIdAsync(int id)
        {
            var estudiante = await repo.GetByIdAsync(id);
            if (estudiante == null)
                return null;
            return new SaveEstudianteDto
            {
                Id = estudiante.Id,
                Nombre = estudiante.Nombre,
                Apellido = estudiante.Apellido,
                Cedula = estudiante.Cedula,
                Carnet = estudiante.Carnet,
                Correo = estudiante.Correo,
                Telefono = estudiante.Telefono,
                CarreraId = estudiante.CarreraId,
                UsuarioId = estudiante.UsuarioId,
            };
        }

        public Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null)
        => repo.GetUsuariosDisponiblesAsync(incluirUsuarioId);

        public Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public Task<int?> GetIdPorUsuarioAsync(int usuarioId)
        => repo.GetIdPorUsuarioAsync(usuarioId);

        public Task<EstudianteTableDto?> GetPerfilAsync(int id)
        => repo.GetPerfilAsync(id);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEstudianteDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var estudiante = await repo.GetByIdAsync(dto.Id.Value);
            if (estudiante == null)
                return (false, "Estudiante no encontrado");

            var error = await ValidarAsync(dto, dto.Id);
            if (error != null)
                return (false, error);

            estudiante.Nombre = dto.Nombre.Trim();
            estudiante.Apellido = dto.Apellido.Trim();
            estudiante.Cedula = dto.Cedula.Trim();
            estudiante.Carnet = dto.Carnet.Trim();
            estudiante.Correo = dto.Correo.Trim();
            estudiante.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            estudiante.CarreraId = dto.CarreraId;
            estudiante.UsuarioId = dto.UsuarioId;

            await repo.UpdateAsync(estudiante);
            return (true, null);
        }

        private async Task<string?> ValidarAsync(SaveEstudianteDto dto, int? excludeId)
        {
            if (await repoCarrera.GetByIdAsync(dto.CarreraId) == null)
                return "Carrera seleccionada no valida";

            if (!await repo.UsuarioExisteAsync(dto.UsuarioId))
                return "La cuenta de usuario seleccionada no existe";

            if (await repo.UsuarioTienePerfilAsync(dto.UsuarioId, excludeId))
                return "Esa cuenta de usuario ya tiene un perfil de estudiante";

            if (await repo.CorreoExisteAsync(dto.Correo.Trim(), excludeId))
                return "Ya existe un estudiante con ese correo";

            if (await repo.CodigoExisteAsync(dto.Carnet.Trim(), dto.UsuarioId, excludeId))
                return "El codigo institucional ya pertenece a otra persona";

            return null;
        }
    }
}

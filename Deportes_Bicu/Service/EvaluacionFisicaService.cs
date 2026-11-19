using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;

namespace Deportes_Bicu.Service
{
    public class EvaluacionFisicaService(IEvaluacionFisicaRepository repo) : IEvaluacionFisicaService
    {
        public async Task<(bool Success, string? ErrorMessage)> CreateAsync(SaveEvaluacionFisicaDto dto)
        {
            var error = await ValidarAsync(dto);
            if (error != null)
                return (false, error);

            await repo.CreateAsync(new EvaluacionFisica
            {
                EstudianteId = dto.EstudianteId,
                Fecha = dto.Fecha,
                Peso = dto.Peso,
                Estatura = dto.Estatura,
                Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim(),
            });
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            var deleted = await repo.DeleteAsync(id);
            return deleted ? (true, null) : (false, "Evaluacion no encontrada");
        }

        public async Task<SaveEvaluacionFisicaDto?> GetByIdAsync(int id)
        {
            var evaluacion = await repo.GetByIdAsync(id);
            if (evaluacion == null)
                return null;
            return new SaveEvaluacionFisicaDto
            {
                Id = evaluacion.Id,
                EstudianteId = evaluacion.EstudianteId,
                Fecha = evaluacion.Fecha,
                Peso = evaluacion.Peso,
                Estatura = evaluacion.Estatura,
                Observaciones = evaluacion.Observaciones,
            };
        }

        public Task<IReadOnlyList<EvaluacionFisicaTableDto>> GetByEstudianteAsync(int estudianteId)
        => repo.GetByEstudianteAsync(estudianteId);

        public Task<IReadOnlyList<OpcionDto>> GetEstudiantesOpcionesAsync()
        => repo.GetEstudiantesOpcionesAsync();

        public Task<DataTableResponseDto<EvaluacionFisicaTableDto>> GetPagedAsync(DataTableRequestDto request)
        => repo.GetPagedAsync(request);

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(SaveEvaluacionFisicaDto dto)
        {
            if (!dto.Id.HasValue)
                return (false, "Id invalido");

            var evaluacion = await repo.GetByIdAsync(dto.Id.Value);
            if (evaluacion == null)
                return (false, "Evaluacion no encontrada");

            var error = await ValidarAsync(dto);
            if (error != null)
                return (false, error);

            evaluacion.EstudianteId = dto.EstudianteId;
            evaluacion.Fecha = dto.Fecha;
            evaluacion.Peso = dto.Peso;
            evaluacion.Estatura = dto.Estatura;
            evaluacion.Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim();

            await repo.UpdateAsync(evaluacion);
            return (true, null);
        }

     
        private async Task<string?> ValidarAsync(SaveEvaluacionFisicaDto dto)
        {
            if (!await repo.EstudianteExisteAsync(dto.EstudianteId))
                return "El estudiante seleccionado no existe";

            if (dto.Fecha > DateOnly.FromDateTime(DateTime.Now))
                return "La fecha de la evaluacion no puede ser futura";

            if (dto.Peso <= 0 || dto.Estatura <= 0)
                return "El peso y la estatura deben ser mayores que 0";

            return null;
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class PortalEstudianteController(IEstudianteService _estudianteService, IInscripcionService _inscripcionService,
        IAsistenciaService _asistenciaService, IEvaluacionFisicaService _evaluacionService) : Controller
    {
        private async Task<int?> GetEstudianteIdAsync()
        {
            var userId = User.GetUserId();
            return userId.HasValue ? await _estudianteService.GetIdPorUsuarioAsync(userId.Value) : null;
        }

        [Authorize(Policy = "portalestudiante.ver")]
        public async Task<IActionResult> Index()
        {
            var estudianteId = await GetEstudianteIdAsync();
            ViewBag.TienePerfil = estudianteId.HasValue;
            return View();
        }

        [Authorize(Policy = "portalestudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisDatos()
        {
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return NotFound(new { message = "Su cuenta no tiene un perfil de estudiante" });
            var perfil = await _estudianteService.GetPerfilAsync(estudianteId.Value);
            return perfil == null ? NotFound(new { message = "Perfil no encontrado" }) : Ok(perfil);
        }

        [Authorize(Policy = "portalestudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisInscripciones()
        {
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return Ok(Array.Empty<InscripcionTableDto>());
            var result = await _inscripcionService.GetByEstudianteAsync(estudianteId.Value);
            return Ok(result);
        }

        [Authorize(Policy = "portalestudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisAsistencias()
        {
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return Ok(Array.Empty<AsistenciaTableDto>());
            var result = await _asistenciaService.GetByEstudianteAsync(estudianteId.Value);
            return Ok(result);
        }

        [Authorize(Policy = "portalestudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisEvaluaciones()
        {
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return Ok(Array.Empty<EvaluacionFisicaTableDto>());
            var result = await _evaluacionService.GetByEstudianteAsync(estudianteId.Value);
            return Ok(result);
        }

        [Authorize(Policy = "portalestudiante.inscribir")]
        [HttpPost]
        public async Task<IActionResult> Inscribir([FromBody] InscribirDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return BadRequest(new { success = false, errorMessage = "Su cuenta no tiene un perfil de estudiante" });

            var (success, errorMessage) = await _inscripcionService.CreateAsync(new SaveInscripcionDto
            {
                EstudianteId = estudianteId.Value,
                DeporteId = dto.DeporteId,
                PeriodoId = dto.PeriodoId,
                CategoriaDeportivaId = dto.CategoriaDeportivaId,
                HorarioIds = dto.HorarioIds
            });
            return success ?
                Ok(new { success = true, message = "Inscripcion registrada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "portalestudiante.inscribir")]
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var estudianteId = await GetEstudianteIdAsync();
            if (!estudianteId.HasValue)
                return BadRequest(new { success = false, errorMessage = "Su cuenta no tiene un perfil de estudiante" });

            var (success, errorMessage) = await _inscripcionService.CambiarEstadoAsync(id, Estados.InscripcionCancelada, estudianteId.Value);
            return success ?
                Ok(new { success = true, message = "Inscripcion cancelada" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

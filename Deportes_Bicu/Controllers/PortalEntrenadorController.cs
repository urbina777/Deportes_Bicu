using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class PortalEntrenadorController(IEntrenadorService _entrenadorService, IAsistenciaService _asistenciaService) : Controller
    {
        private async Task<int?> GetEntrenadorIdAsync()
        {
            var userId = User.GetUserId();
            return userId.HasValue ? await _entrenadorService.GetIdPorUsuarioAsync(userId.Value) : null;
        }

        [Authorize(Policy = "portalentrenador.ver")]
        public async Task<IActionResult> Index()
        {
            var entrenadorId = await GetEntrenadorIdAsync();
            ViewBag.TienePerfil = entrenadorId.HasValue;
            return View();
        }

        [Authorize(Policy = "portalentrenador.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisDatos()
        {
            var entrenadorId = await GetEntrenadorIdAsync();
            if (!entrenadorId.HasValue)
                return NotFound(new { message = "Su cuenta no tiene un perfil de entrenador" });
            var entrenador = await _entrenadorService.GetByIdAsync(entrenadorId.Value);
            if (entrenador == null)
                return NotFound(new { message = "Perfil no encontrado" });
            var deportes = await _entrenadorService.GetDeportesAsync(entrenadorId.Value);
            return Ok(new
            {
                nombre = entrenador.Nombre,
                apellido = entrenador.Apellido,
                carnet = entrenador.Carnet,
                correo = entrenador.Correo,
                deportes = deportes.Asignadas
            });
        }

        [Authorize(Policy = "portalentrenador.ver")]
        [HttpGet]
        public async Task<IActionResult> GetMisHorarios()
        {
            var entrenadorId = await GetEntrenadorIdAsync();
            if (!entrenadorId.HasValue)
                return Ok(Array.Empty<HorarioEntrenadorDto>());
            var result = await _asistenciaService.GetMisHorariosAsync(entrenadorId.Value);
            return Ok(result);
        }

        [Authorize(Policy = "asistencia.registrar")]
        [HttpGet]
        public async Task<IActionResult> GetInscritos(int horarioId, DateOnly fecha)
        {
            var entrenadorId = await GetEntrenadorIdAsync();
            if (!entrenadorId.HasValue)
                return BadRequest(new { success = false, errorMessage = "Su cuenta no tiene un perfil de entrenador" });

            var (data, errorMessage) = await _asistenciaService.GetInscritosAsync(entrenadorId.Value, horarioId, fecha);
            return data != null ? Ok(data) : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "asistencia.registrar")]
        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia([FromBody] GuardarSesionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var entrenadorId = await GetEntrenadorIdAsync();
            if (!entrenadorId.HasValue)
                return BadRequest(new { success = false, errorMessage = "Su cuenta no tiene un perfil de entrenador" });

            var (success, errorMessage) = await _asistenciaService.GuardarSesionAsync(entrenadorId.Value, dto);
            return success ?
                Ok(new { success = true, message = "Asistencia guardada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        //Historial de asistencias de las inscripciones que tienen horarios de este entrenador
        [Authorize(Policy = "portalentrenador.ver")]
        [HttpPost]
        public async Task<IActionResult> GetHistorial([FromBody] DataTableRequestDto request)
        {
            var entrenadorId = await GetEntrenadorIdAsync();
            if (!entrenadorId.HasValue)
                return Ok(new DataTableResponseDto<AsistenciaTableDto> { Draw = request.Draw, Data = [] });
            var result = await _asistenciaService.GetPagedAsync(request, entrenadorId.Value);
            return Ok(result);
        }
    }
}

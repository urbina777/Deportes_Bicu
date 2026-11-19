using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class EntrenadorController(IEntrenadorService _entrenadorService) : Controller
    {
        [Authorize(Policy = "entrenador.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "entrenador.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _entrenadorService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "entrenador.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveEntrenadorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _entrenadorService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Entrenador creado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "entrenador.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _entrenadorService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Entrenador no encontrado" });
            return Ok(result);
        }

        [Authorize(Policy = "entrenador.usuarios")]
        [HttpGet]
        public async Task<IActionResult> GetUsuariosDisponibles(int? incluirId)
        {
            var result = await _entrenadorService.GetUsuariosDisponiblesAsync(incluirId);
            return Ok(result);
        }

        [Authorize(Policy = "entrenador.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveEntrenadorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _entrenadorService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Entrenador actualizado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "entrenador.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _entrenadorService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Entrenador eliminado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "entrenador.ver")]
        [HttpGet]
        public async Task<IActionResult> GetDeportes(int entrenadorId)
        {
            var result = await _entrenadorService.GetDeportesAsync(entrenadorId);
            return Ok(result);
        }

        [Authorize(Policy = "entrenador.asignar")]
        [HttpPost]
        public async Task<IActionResult> AsignarDeporte(int entrenadorId, int deporteId)
        {
            var (success, errorMessage) = await _entrenadorService.AsignarDeporteAsync(entrenadorId, deporteId);
            return success ?
                Ok(new { success = true, message = "Deporte asignado" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "entrenador.asignar")]
        [HttpPost]
        public async Task<IActionResult> QuitarDeporte(int id)
        {
            var (success, errorMessage) = await _entrenadorService.QuitarDeporteAsync(id);
            return success ?
                Ok(new { success = true, message = "Deporte removido" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

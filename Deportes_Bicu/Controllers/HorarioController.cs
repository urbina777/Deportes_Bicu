using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class HorarioController(IHorarioService _horarioService, IInstalacionService _instalacionService) : Controller
    {
        [Authorize(Policy = "horario.ver")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Asignaciones = await _horarioService.GetAsignacionesOpcionesAsync();
            ViewBag.Instalaciones = await _instalacionService.GetOpcionesAsync(true);
            return View();
        }

        [Authorize(Policy = "horario.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _horarioService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "horario.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveHorarioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _horarioService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Horario creado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "horario.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _horarioService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Horario no encontrado" });
            return Ok(result);
        }

        [Authorize(Policy = "horario.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveHorarioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _horarioService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Horario actualizado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "horario.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _horarioService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Horario eliminado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

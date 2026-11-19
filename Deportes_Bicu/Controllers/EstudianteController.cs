using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class EstudianteController(IEstudianteService _estudianteService, ICarreraService _carreraService) : Controller
    {
        [Authorize(Policy = "estudiante.ver")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Carreras = await _carreraService.GetAllAsync();
            return View();
        }

        [Authorize(Policy = "estudiante.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _estudianteService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "estudiante.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveEstudianteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _estudianteService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Estudiante creado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "estudiante.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _estudianteService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Estudiante no encontrado" });
            return Ok(result);
        }

        [Authorize(Policy = "estudiante.usuarios")]
        [HttpGet]
        public async Task<IActionResult> GetUsuariosDisponibles(int? incluirId)
        {
            var result = await _estudianteService.GetUsuariosDisponiblesAsync(incluirId);
            return Ok(result);
        }

        [Authorize(Policy = "estudiante.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveEstudianteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _estudianteService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Estudiante actualizado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "estudiante.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _estudianteService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Estudiante eliminado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

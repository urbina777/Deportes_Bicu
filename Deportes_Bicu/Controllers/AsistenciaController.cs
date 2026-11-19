using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class AsistenciaController(IAsistenciaService _asistenciaService) : Controller
    {
        [Authorize(Policy = "asistencia.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "asistencia.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _asistenciaService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "asistencia.crear")]
        [HttpGet]
        public async Task<IActionResult> GetInscripciones()
        {
            var result = await _asistenciaService.GetInscripcionesActivasOpcionesAsync();
            return Ok(result);
        }

        [Authorize(Policy = "asistencia.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveAsistenciaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _asistenciaService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Asistencia registrada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "asistencia.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _asistenciaService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Asistencia no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "asistencia.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveAsistenciaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _asistenciaService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Asistencia actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "asistencia.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _asistenciaService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Asistencia eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

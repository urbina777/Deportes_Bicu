using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class InstalacionController(IInstalacionService _service) : Controller
    {
        [Authorize(Policy = "instalacion.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "instalacion.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "instalacion.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveInstalacionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Instalación creada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "instalacion.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Instalación no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "instalacion.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveInstalacionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Instalación actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "instalacion.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _service.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Instalación eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

    }
}

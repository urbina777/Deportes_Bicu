using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class PeriodoController(IPeriodoService _service) : Controller
    {
        [Authorize(Policy = "periodo.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "periodo.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "periodo.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SavePeriodoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Período creado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "periodo.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Período no encontrado" });
            return Ok(result);
        }

        [Authorize(Policy = "periodo.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SavePeriodoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Período actualizado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "periodo.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _service.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Período eliminado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

    }
}

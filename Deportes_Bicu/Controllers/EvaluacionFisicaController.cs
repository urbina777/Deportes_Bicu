using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class EvaluacionFisicaController(IEvaluacionFisicaService _evaluacionService) : Controller
    {
        [Authorize(Policy = "evaluacion.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "evaluacion.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _evaluacionService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "evaluacion.opciones")]
        [HttpGet]
        public async Task<IActionResult> GetEstudiantes()
        {
            var result = await _evaluacionService.GetEstudiantesOpcionesAsync();
            return Ok(result);
        }

        [Authorize(Policy = "evaluacion.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveEvaluacionFisicaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _evaluacionService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Evaluacion registrada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "evaluacion.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _evaluacionService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Evaluacion no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "evaluacion.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveEvaluacionFisicaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _evaluacionService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Evaluacion actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "evaluacion.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _evaluacionService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Evaluacion eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

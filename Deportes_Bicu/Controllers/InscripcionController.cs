using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class InscripcionController(IInscripcionService _inscripcionService, IDeporteService _deporteService,
        IPeriodoService _periodoService) : Controller
    {
        [Authorize(Policy = "inscripcion.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "inscripcion.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _inscripcionService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "inscripcion.opciones")]
        [HttpGet]
        public async Task<IActionResult> GetOpciones()
        {
            var deportes = await _deporteService.GetOpcionesAsync(true);
            var periodos = await _periodoService.GetOpcionesAsync(true);
            return Ok(new { deportes, periodos });
        }

        [Authorize(Policy = "inscripcion.crear")]
        [HttpGet]
        public async Task<IActionResult> GetEstudiantes()
        {
            var result = await _inscripcionService.GetEstudiantesOpcionesAsync();
            return Ok(result);
        }

        [Authorize(Policy = "inscripcion.opciones")]
        [HttpGet]
        public async Task<IActionResult> GetCategorias(int deporteId)
        {
            var result = await _inscripcionService.GetCategoriasAsync(deporteId);
            return Ok(result);
        }

        [Authorize(Policy = "inscripcion.opciones")]
        [HttpGet]
        public async Task<IActionResult> GetHorariosDisponibles(int deporteId, int periodoId)
        {
            var result = await _inscripcionService.GetHorariosDisponiblesAsync(deporteId, periodoId);
            return Ok(result);
        }

        [Authorize(Policy = "inscripcion.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveInscripcionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _inscripcionService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Inscripcion registrada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "inscripcion.actualizar")]
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var (success, errorMessage) = await _inscripcionService.CambiarEstadoAsync(id, estado);
            return success ?
                Ok(new { success = true, message = "Estado de la inscripcion actualizado" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "inscripcion.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _inscripcionService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Inscripcion eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

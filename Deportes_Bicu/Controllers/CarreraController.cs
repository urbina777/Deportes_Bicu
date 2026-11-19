using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class CarreraController(ICarreraService _carreraService, IFacultadService _facultadService) : Controller
    {
        [Authorize(Policy = "carrera.ver")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Facultades = await _facultadService.GetAllAsync();
            return View();
        }

        [Authorize(Policy = "carrera.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _carreraService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "carrera.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveCarreraDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _carreraService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Carrera creada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "carrera.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _carreraService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Carrera no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "carrera.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveCarreraDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _carreraService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Carrera actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "carrera.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _carreraService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Carrera eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class FacultadController(IFacultadService _facultadService) : Controller
    {
        [Authorize(Policy = "facultad.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "facultad.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _facultadService.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "facultad.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveFacultadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _facultadService.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Facultad creada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "facultad.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _facultadService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Facultad no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "facultad.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveFacultadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _facultadService.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Facultad actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "facultad.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _facultadService.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Facultad eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

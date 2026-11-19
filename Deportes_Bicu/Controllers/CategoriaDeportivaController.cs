using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class CategoriaDeportivaController(ICategoriaDeportivaService _service) : Controller
    {
        [Authorize(Policy = "categoria.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "categoria.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "categoria.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveCategoriaDeportivaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Categoría deportiva creada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "categoria.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Categoría deportiva no encontrada" });
            return Ok(result);
        }

        [Authorize(Policy = "categoria.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveCategoriaDeportivaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Categoría deportiva actualizada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "categoria.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _service.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Categoría deportiva eliminada satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Service;

namespace Deportes_Bicu.Controllers
{
    public class DeporteController(IDeporteService _service) : Controller
    {
        [Authorize(Policy = "deporte.ver")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "deporte.ver")]
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] DataTableRequestDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }

        [Authorize(Policy = "deporte.crear")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveDeporteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.CreateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Deporte creado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "deporte.ver")]
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Deporte no encontrado" });
            return Ok(result);
        }

        [Authorize(Policy = "deporte.actualizar")]
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SaveDeporteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Error en los datos enviados" });
            var (success, errorMessage) = await _service.UpdateAsync(dto);
            return success ?
                Ok(new { success = true, message = "Deporte actualizado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "deporte.borrar")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, errorMessage) = await _service.DeleteAsync(id);
            return success ?
                Ok(new { success = true, message = "Deporte eliminado satisfactoriamente" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "deporte.ver")]
        [HttpGet]
        public async Task<IActionResult> GetCategorias(int deporteId)
        {
            var result = await _service.GetCategoriasAsync(deporteId);
            return Ok(result);
        }

        [Authorize(Policy = "deporte.asignar")]
        [HttpPost]
        public async Task<IActionResult> AsignarCategoria(int deporteId, int categoriaId)
        {
            var (success, errorMessage) = await _service.AsignarCategoriaAsync(deporteId, categoriaId);
            return success ?
                Ok(new { success = true, message = "Categoria asignada" })
                 : BadRequest(new { success = false, errorMessage });
        }

        [Authorize(Policy = "deporte.asignar")]
        [HttpPost]
        public async Task<IActionResult> QuitarCategoria(int id)
        {
            var (success, errorMessage) = await _service.QuitarCategoriaAsync(id);
            return success ?
                Ok(new { success = true, message = "Categoria removida" })
                 : BadRequest(new { success = false, errorMessage });
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveAsistenciaDto
    {
        public int? Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una inscripcion.")]
        public int InscripcionId { get; set; }
        public DateOnly Fecha { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }
}

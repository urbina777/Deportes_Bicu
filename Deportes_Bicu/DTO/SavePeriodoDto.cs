using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SavePeriodoDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }
}

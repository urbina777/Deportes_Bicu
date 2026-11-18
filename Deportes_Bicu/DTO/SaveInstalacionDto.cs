using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveInstalacionDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? Ubicacion { get; set; }
        [Range(1, 10000, ErrorMessage = "La capacidad debe estar entre 1 y 10000.")]
        public int Capacidad { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }
}

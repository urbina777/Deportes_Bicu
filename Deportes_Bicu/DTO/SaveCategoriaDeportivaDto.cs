using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveCategoriaDeportivaDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;
        [Range(0, 120, ErrorMessage = "La edad mínima debe estar entre 0 y 120.")]
        public int EdadMinima { get; set; }
        [Range(0, 120, ErrorMessage = "La edad máxima debe estar entre 0 y 120.")]
        public int EdadMaxima { get; set; }
    }
}

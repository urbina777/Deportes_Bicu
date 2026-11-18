using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveFacultadDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre de la facultad es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }
}

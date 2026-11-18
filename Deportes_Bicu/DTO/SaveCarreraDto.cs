using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveCarreraDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una facultad.")]
        public int FacultadId { get; set; }
    }
}

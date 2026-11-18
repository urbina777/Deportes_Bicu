using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveHorarioDto
    {
        public int? Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione la asignacion entrenador - deporte.")]
        public int EntrenadorDeporteId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una instalacion.")]
        public int InstalacionId { get; set; }
        [Required(ErrorMessage = "El dia es obligatorio.")]
        [MaxLength(20)]
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }
}

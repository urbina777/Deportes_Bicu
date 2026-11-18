using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveEvaluacionFisicaDto
    {
        public int? Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estudiante.")]
        public int EstudianteId { get; set; }
        public DateOnly Fecha { get; set; }
     
        [Range(0.01, 999.99, ErrorMessage = "El peso debe ser mayor que 0 (kg).")]
        public decimal Peso { get; set; }
        [Range(0.01, 99.99, ErrorMessage = "La estatura debe ser mayor que 0 (metros).")]
        public decimal Estatura { get; set; }
        [MaxLength(250)]
        public string? Observaciones { get; set; }
    }
}

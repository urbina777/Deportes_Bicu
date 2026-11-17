namespace Deportes_Bicu.Models
{
    public class EvaluacionFisica
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public decimal Peso { get; set; }
        public decimal Estatura { get; set; }
        public string? Observaciones { get; set; }
        public int EstudianteId { get; set; }

        public virtual Estudiante Estudiante { get; set; } = null!;
    }
}

namespace Deportes_Bicu.DTOS
{
    public class EvaluacionFisicaTableDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public decimal Peso { get; set; }
        public decimal Estatura { get; set; }
        public string? Observaciones { get; set; }
    }
}

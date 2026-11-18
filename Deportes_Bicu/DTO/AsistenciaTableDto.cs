namespace Deportes_Bicu.DTOS
{
    public class AsistenciaTableDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int InscripcionId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string DeporteNombre { get; set; } = string.Empty;
        public string PeriodoNombre { get; set; } = string.Empty;
    }
}

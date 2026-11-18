namespace Deportes_Bicu.DTOS
{
    public class PeriodoTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}

namespace Deportes_Bicu.Models
{
    public class Asistencia
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int InscripcionId { get; set; }

        public virtual Inscripcion Inscripcion { get; set; } = null!;
    }
}

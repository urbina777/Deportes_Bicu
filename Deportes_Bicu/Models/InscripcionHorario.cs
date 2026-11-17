namespace Deportes_Bicu.Models
{
    public class InscripcionHorario
    {
        public int Id { get; set; }
        public int InscripcionId { get; set; }
        public int HorarioId { get; set; }

        public virtual Inscripcion Inscripcion { get; set; } = null!;
        public virtual Horario Horario { get; set; } = null!;
    }
}

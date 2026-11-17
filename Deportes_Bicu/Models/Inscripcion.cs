namespace Deportes_Bicu.Models
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int EstudianteId { get; set; }
        public int DeporteId { get; set; }
        public int PeriodoId { get; set; }
        public int DeporteCategoriaId { get; set; }

        public virtual Estudiante Estudiante { get; set; } = null!;
        public virtual Deporte Deporte { get; set; } = null!;
        public virtual Periodo Periodo { get; set; } = null!;
        public virtual DeporteCategoria DeporteCategoria { get; set; } = null!;
        public ICollection<InscripcionHorario> InscripcionHorario { get; set; } = [];
        public ICollection<Asistencia> Asistencia { get; set; } = [];
    }
}

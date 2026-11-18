namespace Deportes_Bicu.Models
{
    public class Estudiante
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public int CarreraId { get; set; }
        public int UsuarioId { get; set; }

        public virtual Carrera Carrera { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
        public ICollection<Inscripcion> Inscripcion { get; set; } = [];
        public ICollection<EvaluacionFisica> EvaluacionFisica { get; set; } = [];
    }
}

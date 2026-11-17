namespace Deportes_Bicu.Models
{
    public class Carrera
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int FacultadId { get; set; }

        public virtual Facultad Facultad { get; set; } = null!;
        public ICollection<Estudiante> Estudiante { get; set; } = [];
    }
}

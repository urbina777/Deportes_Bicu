namespace Deportes_Bicu.Models
{
    public class DeporteCategoria
    {
        public int Id { get; set; }
        public int DeporteId { get; set; }
        public int CategoriaDeportivaId { get; set; }

        public virtual Deporte Deporte { get; set; } = null!;
        public virtual CategoriaDeportiva CategoriaDeportiva { get; set; } = null!;
        public ICollection<Inscripcion> Inscripcion { get; set; } = [];
    }
}

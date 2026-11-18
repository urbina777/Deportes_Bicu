namespace Deportes_Bicu.Models
{
    public class CategoriaDeportiva
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int EdadMinima { get; set; }
        public int EdadMaxima { get; set; }

        public ICollection<DeporteCategoria> DeporteCategoria { get; set; } = [];
    }
}

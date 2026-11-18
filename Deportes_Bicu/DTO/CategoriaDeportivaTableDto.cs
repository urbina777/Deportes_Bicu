namespace Deportes_Bicu.DTOS
{
    public class CategoriaDeportivaTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int EdadMinima { get; set; }
        public int EdadMaxima { get; set; }
    }
}

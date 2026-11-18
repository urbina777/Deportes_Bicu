namespace Deportes_Bicu.Models
{
    public class Facultad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public ICollection<Carrera> Carrera { get; set; } = [];
    }
}

namespace Deportes_Bicu.Models
{
    public class Deporte
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string Estado { get; set; } = string.Empty;

        public ICollection<EntrenadorDeporte> EntrenadorDeporte { get; set; } = [];
        public ICollection<DeporteCategoria> DeporteCategoria { get; set; } = [];
        public ICollection<Inscripcion> Inscripcion { get; set; } = [];
    }
}

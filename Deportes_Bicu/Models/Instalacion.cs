namespace Deportes_Bicu.Models
{
    public class Instalacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public int Capacidad { get; set; }
        public string Estado { get; set; } = string.Empty;

        public ICollection<Horario> Horario { get; set; } = [];
    }
}

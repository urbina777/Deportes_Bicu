namespace Deportes_Bicu.DTOS
{
    public class InstalacionTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Ubicacion { get; set; }
        public int Capacidad { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}

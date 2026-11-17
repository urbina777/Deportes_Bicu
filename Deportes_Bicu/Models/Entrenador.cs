namespace Deportes_Bicu.Models
{
    public class Entrenador
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Correo { get; set; } = string.Empty;
        public int UsuarioId { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;
        public ICollection<EntrenadorDeporte> EntrenadorDeporte { get; set; } = [];
    }
}

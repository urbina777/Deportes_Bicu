namespace Deportes_Bicu.DTOS
{
    public class EstudianteTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int CarreraId { get; set; }
        public string CarreraNombre { get; set; } = string.Empty;
        public string FacultadNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}

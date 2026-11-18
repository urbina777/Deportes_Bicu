namespace Deportes_Bicu.DTOS
{
    public class CarreraTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int FacultadId { get; set; }
        public string FacultadNombre { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
    }
}

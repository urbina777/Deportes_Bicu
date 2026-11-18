namespace Deportes_Bicu.DTOS
{
    public class FacultadTableDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int TotalCarreras { get; set; }
    }
}

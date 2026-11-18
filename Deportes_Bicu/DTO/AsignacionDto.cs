namespace Deportes_Bicu.DTOS
{
   
    public class AsignacionDto
    {
      
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string? Detalle { get; set; }
    }

    
    public class AsignacionesDto
    {
        public List<AsignacionDto> Asignadas { get; set; } = [];
        public List<OpcionDto> Disponibles { get; set; } = [];
    }
}

namespace Deportes_Bicu.DTOS
{
    public class HorarioTableDto
    {
        public int Id { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int EntrenadorDeporteId { get; set; }
        public string DeporteNombre { get; set; } = string.Empty;
        public string EntrenadorNombre { get; set; } = string.Empty;
        public int InstalacionId { get; set; }
        public string InstalacionNombre { get; set; } = string.Empty;
        public int Capacidad { get; set; }
       
        public int Inscritos { get; set; }
    }
}

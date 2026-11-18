namespace Deportes_Bicu.DTOS
{
   
    public class HorarioDisponibleDto
    {
        public int Id { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string InstalacionNombre { get; set; } = string.Empty;
        public string EntrenadorNombre { get; set; } = string.Empty;
        public int Capacidad { get; set; }
        public int Inscritos { get; set; }
    }

    
    public class HorarioInfoDto
    {
        public int Id { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int DeporteId { get; set; }
        public string DeporteNombre { get; set; } = string.Empty;
        public string InstalacionNombre { get; set; } = string.Empty;
        public bool InstalacionActiva { get; set; }
        public int Capacidad { get; set; }
    }
}

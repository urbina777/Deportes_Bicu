namespace Deportes_Bicu.DTOS
{
    public class HorarioResumenDto
    {
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string InstalacionNombre { get; set; } = string.Empty;
        public string EntrenadorNombre { get; set; } = string.Empty;
    }

    public class InscripcionTableDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int EstudianteId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string DeporteNombre { get; set; } = string.Empty;
        public string CategoriaNombre { get; set; } = string.Empty;
        public string PeriodoNombre { get; set; } = string.Empty;
        public int TotalAsistencias { get; set; }
        public List<HorarioResumenDto> Horarios { get; set; } = [];
    }
}

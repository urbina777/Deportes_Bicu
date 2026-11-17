namespace Deportes_Bicu.Models
{
    public class Horario
    {
        public int Id { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int InstalacionId { get; set; }
        public int EntrenadorDeporteId { get; set; }

        public virtual Instalacion Instalacion { get; set; } = null!;
        public virtual EntrenadorDeporte EntrenadorDeporte { get; set; } = null!;
        public ICollection<InscripcionHorario> InscripcionHorario { get; set; } = [];
    }
}

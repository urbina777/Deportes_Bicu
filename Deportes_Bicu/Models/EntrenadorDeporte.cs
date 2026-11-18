namespace Deportes_Bicu.Models
{
    public class EntrenadorDeporte
    {
        public int Id { get; set; }
        public int EntrenadorId { get; set; }
        public int DeporteId { get; set; }

        public virtual Entrenador Entrenador { get; set; } = null!;
        public virtual Deporte Deporte { get; set; } = null!;
        public ICollection<Horario> Horario { get; set; } = [];
    }
}

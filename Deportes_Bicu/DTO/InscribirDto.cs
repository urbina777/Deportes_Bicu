using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    
    public class InscribirDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un deporte.")]
        public int DeporteId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un periodo.")]
        public int PeriodoId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoria.")]
        public int CategoriaDeportivaId { get; set; }
        public List<int> HorarioIds { get; set; } = [];
    }
}

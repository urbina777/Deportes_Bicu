using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    
    public class AsistenciaInscritoDto
    {
        public int InscripcionId { get; set; }
        public string EstudianteNombre { get; set; } = string.Empty;
        public string Carnet { get; set; } = string.Empty;
        public string? Estado { get; set; }
    }

    public class RegistroAsistenciaDto
    {
        public int InscripcionId { get; set; }
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = string.Empty;
    }

    public class GuardarSesionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un horario.")]
        public int HorarioId { get; set; }
        public DateOnly Fecha { get; set; }
        public List<RegistroAsistenciaDto> Registros { get; set; } = [];
    }

    public class HorarioEntrenadorDto
    {
        public int Id { get; set; }
        public string Dia { get; set; } = string.Empty;
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public string DeporteNombre { get; set; } = string.Empty;
        public string InstalacionNombre { get; set; } = string.Empty;
    }

    
    public class InscripcionAsistenciaInfoDto
    {
        public int Id { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public List<string> Dias { get; set; } = [];
    }
}

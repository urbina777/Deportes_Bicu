using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class SaveEstudianteDto
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;
        [Required(ErrorMessage = "La cedula es obligatoria")]
        [MaxLength(20)]
        public string Cedula { get; set; } = string.Empty;
        [Required(ErrorMessage = "El codigo institucional es obligatorio")]
        [MaxLength(30)]
        public string Carnet { get; set; } = string.Empty;
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        [MaxLength(100)]
        public string Correo { get; set; } = string.Empty;
        [MaxLength(20)]
        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Ingrese un telefono valido")]
        public string? Telefono { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una carrera.")]
        public int CarreraId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una cuenta de usuario.")]
        public int UsuarioId { get; set; }
    }
}

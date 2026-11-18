using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class CreatePermissionDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(150)]
        [RegularExpression(@"^[a-z][a-z0-9._]*$", ErrorMessage = "Use el formato Recurso.accion")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Descripcion { get; set; }
    }
}

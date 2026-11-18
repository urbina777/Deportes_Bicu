using System.ComponentModel.DataAnnotations;

namespace Deportes_Bicu.DTOS
{
    public class CreateRolDto
    {
        [Required(ErrorMessage = "El nombre del Rol es Requerido")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

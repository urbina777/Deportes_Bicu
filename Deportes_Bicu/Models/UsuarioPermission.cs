namespace Deportes_Bicu.Models
{
    public class UsuarioPermission
    {
        public int UsuarioId { get; set; }
        public int PermissionId { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
        public bool isGranted { get; set; }
    }
}

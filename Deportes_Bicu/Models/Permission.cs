namespace Deportes_Bicu.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Nombre { get; set; }=string.Empty;
        public string? Descripcion { get; set; }
        public ICollection<UsuarioPermission> UsuarioPermission { get; set; } = [];
        public ICollection<RolePermission> RolePermission { get; set; } = [];
    }
}

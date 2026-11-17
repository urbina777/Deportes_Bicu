namespace Deportes_Bicu.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }= string.Empty;
        public ICollection<Usuario> Usuario { get; set; } = [];
        public ICollection<RolePermission> RolePermission { get; set; } = [];

    }
}

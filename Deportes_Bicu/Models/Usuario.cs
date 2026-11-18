namespace Deportes_Bicu.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string userName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? EmailConfirmedToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryTime { get; set; }

        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public ICollection<UsuarioPermission> UsuarioPermission { get; set; } = [];
    }
}

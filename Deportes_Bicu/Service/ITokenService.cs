using Deportes_Bicu.Models;

namespace Deportes_Bicu.Service
{
    public interface ITokenService
    {
        string GenerateAccessToken(Usuario usuario);
        string GenerateRefreshToken();
        string GeneratePasswordResetToken();

    }
}

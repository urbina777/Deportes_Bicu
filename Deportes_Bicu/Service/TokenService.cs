using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Deportes_Bicu.Models.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Deportes_Bicu.Service
{
    public class TokenService(IOptions<JwtSettings> _JwtSettings) : ITokenService
    {
        private readonly JwtSettings _jwt = _JwtSettings.Value;
        public string GenerateAccessToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.userName),
                new(ClaimTypes.Email, usuario.Email),
                new(ClaimTypes.Role, usuario.Role?.Descripcion??"Usuario")
            };

            foreach (var permissions in EffectivePermission.FromUser(usuario))
            {
                claims.Add(new Claim(ClaimsPrincipalExtension.PermisionClaimType, permissions));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwt.TokenValidateInMinutes),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GeneratePasswordResetToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}

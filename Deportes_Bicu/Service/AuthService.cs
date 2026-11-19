using AutoMapper;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;
using Deportes_Bicu.Repositories;
using System.Security.Cryptography;
using System.Security.Policy;

namespace Deportes_Bicu.Service
{
    public class AuthService(IUsuarioRepository userRepo, IRoleRepository roleRepo, ITokenService tokenService, IEmailService emailService, IMapper mapper) : IAuthService
    {
        private readonly IUsuarioRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IEmailService _emailService = emailService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> ChangePasswordAsync(string userName, ChangePasswordDto dto)
        {
            var user = await _userRepo.GetUsuarioByUserName(userName);
            if (user == null || !_userRepo.ValidatePassword(user, dto.CurrentPassword))
                return false;
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.NewPassword);
            await _userRepo.SaveAsync();
            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);
            if(user == null || user.EmailConfirmedToken != token) 
                return false;
            var decodedToken =Uri.UnescapeDataString(token.Trim());
            if (!string.Equals(user.EmailConfirmedToken, decodedToken, StringComparison.OrdinalIgnoreCase))
                return false;
            user.EmailConfirmed = true;
            user.EmailConfirmedToken = null;
            await _userRepo.SaveAsync();
            return true;

        }

        public async Task<(bool Success, LoginResponseDto? result, string? ErrorMessage)> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepo.GetUsuarioByEmail(dto.Email);
            if (user == null || !_userRepo.ValidatePassword(user, dto.Password))
                return (false, null, "Credenciales Invalidas");
            if(!user.EmailConfirmed)
                return (false, null, "Debe confirmar su correo");
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            await _userRepo.SaveAsync();
            var result = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(15),
            };
            return (true, result, null);
        }

        public async Task<LoginResponseDto?> RefreshAccessTokenAsync(string refeshToken)
        {
            var user = await _userRepo.GetUsuarioByRefreshTokenAsync(refeshToken);
            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userRepo.SaveAsync();
            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                RefreshTokenExpiresAt = user.RefreshTokenExpiryTime
            };
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegisterRequestDto dto)
        {
            var exist = await _userRepo.GetUsuarioByEmail(dto.Email);
            if (exist != null)
                return(false, "Correo en uso, Intente usar otro diferente");
            var usuarioRole = await _roleRepo.GetByNameAsync("Usuario");
                if (usuarioRole == null)
                return (false, "El Rol Usuario no esta configurado");
            var user = _mapper.Map<Usuario>(dto);
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
            user.RoleId = usuarioRole.Id;

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(tokenBytes);
            var encodedToken = Uri.EscapeDataString(rawToken);

            user.EmailConfirmedToken = rawToken;
            user.EmailConfirmed = false;
            await _userRepo.CreateAsync(user);
            await _userRepo.SaveAsync();

            string confirmationLink = $"https://localhost:7100/Auth/ConfirmEmail?email={user.Email}&token={encodedToken}";
            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <p> Hola </p>
                <p> 
                Haz CLick en el siguinte boton para confirmar tu cuenta
                </p>

                <p style="margin:24px 0;">
                    <a href="{confirmationLink}" stryle='background:#007BBFF'color:white; padding: 10px 20px; border-radius:5px; text-decoration:none </a>
                </p>
                </html>
                """;
            _emailService.SendPasswordResetEmail(user.Email, htmlBody);
            return (true, null);
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);
            if(user == null || user.EmailConfirmed)
                return false;

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var rawToken = Convert.ToBase64String(tokenBytes);
            var encodedToken = Uri.EscapeDataString(rawToken);

            user.EmailConfirmedToken = rawToken;
            await _userRepo.SaveAsync();

            string confirmationLink = $"https://localhost:7100/Auth/ConfirmEmail?email={user.Email}&token={encodedToken}";
            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <p> Hola </p>
                <p> 
                Haz CLick en el siguinte boton para confirmar tu cuenta
                </p>

                <p style="margin:24px 0;">
                    <a href="{confirmationLink}" stryle='background:#007BBFF'color:white; padding: 10px 20px; border-radius:5px; text-decoration:none </a>
                </p>
                </html>
                """;
            _emailService.SendPasswordResetEmail(user.Email, htmlBody);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var user = await _userRepo.GetUsuarioByEmail(dto.Email);
            if (user == null) 
                return false;

            var providerToken = Uri.UnescapeDataString(dto.Token ?? string.Empty);
            var storedToken = user.PasswordResetToken ?? string.Empty;
            
            
            if(!storedToken.Equals(providerToken,StringComparison.Ordinal))
                return false;

            if(!user.PasswordResetTokenExpiryTime.HasValue || user.PasswordResetTokenExpiryTime.Value < DateTime.Now)
                return false;

            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiryTime = null;

            await _userRepo.SaveAsync();
            return true;


        }

        public async Task<bool> SendResetPasswordLinkAsync(string email)
        {
            var user = await _userRepo.GetUsuarioByEmail(email);
            if (user == null) 
                return false;

            var token = _tokenService.GeneratePasswordResetToken();
            var encodedToken = Uri.EscapeDataString(token);
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiryTime = DateTime
                .UtcNow.AddHours(1);
            await _userRepo.SaveAsync();

            string confirmationLink = $"https://localhost:7100/Auth/ResetPassword?email={user.Email}&token={encodedToken}";
            string htmlBody = $"""
                <!DOCTYPE html>
                <html>
                <p> Hola </p>
                <p> 
                Haz CLick en el siguinte boton para confirmar tu cuenta
                </p>

                <p>
                    <a href="{confirmationLink}" stryle='background:#007BBFF'color:white; padding: 10px 20px; border-radius:5px; text-decoration:none </a>
                </p>
                </html>
                """;
            _emailService.SendPasswordResetEmail(user.Email, htmlBody);
            return true;
        }
    }
}

using Microsoft.Extensions.Options;
using Deportes_Bicu.Models.Settings;
using System.Net.Mail;
using System.Net;


namespace Deportes_Bicu.Service
{
    public class EmailService(IOptions<SmtpSettings> _SmtpSettings) : IEmailService
    {
        private readonly SmtpSettings _smtp= _SmtpSettings.Value;
        public void SendPasswordResetEmail(string toEmail, string body)
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
                EnableSsl = true
            };
            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.User, "Soporte de Autenticación"),
                Subject = "Token de Confirmacion/Recuperación",
                Body = body,
                IsBodyHtml = true,
            };
            mail.To.Add(toEmail);
            client.Send(mail);
        }
    }
}

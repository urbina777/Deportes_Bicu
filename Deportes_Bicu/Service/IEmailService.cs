namespace Deportes_Bicu.Service
{
    public interface IEmailService
    {
        void SendPasswordResetEmail(string toEmail, string body);
    }
}

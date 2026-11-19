namespace Deportes_Bicu.Models.Settings
{
    public class SmtpSettings
    {
        //Requisitos para server de correo de Google
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl{ get; set; }
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}

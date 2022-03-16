namespace VerifyBot.Services.Email.Smtp.Configuration
{
    public class SmtpEmailOptions
    {
        public const string Name = "SmtpEmail";
        
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; }
        public bool BodyIsHtml { get; set; }
    }
}
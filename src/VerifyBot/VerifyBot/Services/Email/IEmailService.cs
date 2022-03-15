using System.Threading.Tasks;

namespace VerifyBot.Services.Email
{
    public interface IEmailService
    {
        public Task SendVerificationEmailAsync(string address, string token);
    }
}
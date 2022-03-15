using System.Threading.Tasks;

namespace VerifyBot.Services.Email
{
    public interface IEmailService
    {
        public Task SendVerificationEmail(string address, string token);
    }
}
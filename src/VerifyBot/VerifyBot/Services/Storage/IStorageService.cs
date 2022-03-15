using System.Threading.Tasks;

namespace VerifyBot.Services.Storage
{
    public interface IStorageService
    {
        Task AddPendingVerificationAsync(ulong userId, string token, string username);
    }
}
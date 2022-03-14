using System.Threading.Tasks;

namespace VerifyBot.Services.Storage
{
    public interface IStorageService
    {
        Task AddPendingVerificationAsync(string username);
    }
}
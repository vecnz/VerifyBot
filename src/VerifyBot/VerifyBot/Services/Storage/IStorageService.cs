using System.Threading.Tasks;
using VerifyBot.Services.Storage.Models;

namespace VerifyBot.Services.Storage
{
    public interface IStorageService
    {
        Task AddPendingVerificationAsync(PendingVerification pendingVerification);
    }
}
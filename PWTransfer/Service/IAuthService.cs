using Shared;
using System.Threading.Tasks;

namespace PWTransfer.Service
{
    public interface IAuthService
    {
        Task<UserId> GetUserId();
    }
}

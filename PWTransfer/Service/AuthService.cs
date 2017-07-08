using System;
using System.Threading.Tasks;
using Shared;

namespace PWTransfer.Service
{
    public class AuthService : IAuthService
    {
        private readonly string _authServiceUrl;

        public AuthService(string authServiceUrl)
        {
            _authServiceUrl = authServiceUrl;
        }

        public Task<UserId> GetUserId()
        {
            throw new NotImplementedException();
        }
    }
}

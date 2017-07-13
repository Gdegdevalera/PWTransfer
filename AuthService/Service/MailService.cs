using System;
using System.Threading.Tasks;

namespace AuthService.Service
{
    public class MailService : IMailService
    {
        public Task SendEmailConfirmation(string email, string confirmationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailForgotPassword(string email, string passwordToken)
        {
            throw new NotImplementedException();
        }
    }
}

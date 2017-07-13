using System.Threading.Tasks;

namespace AuthService.Service
{
    public interface IMailService
    {
        Task SendEmailConfirmation(string email, string confirmationToken);

        Task SendEmailForgotPassword(string email, string passwordToken);
    }
}

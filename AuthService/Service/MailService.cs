using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AuthService.Service
{
    public class MailService : IMailService
    {
        private readonly NETCore.MailKit.Core.IEmailService _emailService;
        private readonly string _entryPointUrl;

        public MailService(NETCore.MailKit.Core.IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _entryPointUrl = config["Auth:EntryPointUrl"];
        }

        public async Task SendEmailConfirmation(string mailTo, string confirmationToken)
        {
            var subject = "Подтверждение регистрации";
            var confirmationUrl = _entryPointUrl + "/confirm/" + confirmationToken;
            var body = $"Для завершения процедуры регистрации пройдите по <a href='{confirmationUrl}'>ссылке</a>";

            await _emailService.SendAsync(mailTo, subject, body, isHtml: true);
        }

        public async Task SendEmailForgotPassword(string mailTo, string passwordToken)
        {
            var subject = "Сброс пароля";
            var confirmationUrl = _entryPointUrl + "/resetPassword/" + passwordToken;
            var body = $"Для смены пароля пройдите по <a href='{confirmationUrl}'>ссылке</a>.<br>" +
                "Ссылка будет дейстовать втечение одного часа.";

            await _emailService.SendAsync(mailTo, subject, body, isHtml: true);
        }
    }
}

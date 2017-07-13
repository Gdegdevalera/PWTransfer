using SmtpServer.Storage;
using System.Collections.Generic;
using SmtpServer;
using SmtpServer.Protocol;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Authentication;
using SmtpServer.Mail;
using System.IO;
using MimeKit;

namespace PWTransfer.Tests
{
    public class TestMessageStore : MessageStore
    {
        public List<MimeMessage> Transactions { get; private set; }

        public TestMessageStore()
        {
            Transactions = new List<MimeMessage>();
        }

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var textMessage = (ITextMessage)transaction.Message;
            var message = MimeMessage.Load(textMessage.Content);

            Transactions.Add(message);
            return Task.FromResult(SmtpResponse.Ok);
        }
    }

    public class SampleUserAuthenticator : IUserAuthenticator
    {
        public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    public class SampleUserAuthenticatorFactory : IUserAuthenticatorFactory
    {
        public IUserAuthenticator CreateInstance(ISessionContext context)
        {
            return new SampleUserAuthenticator();
        }
    }
}

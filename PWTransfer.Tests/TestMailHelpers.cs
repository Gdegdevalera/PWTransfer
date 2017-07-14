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
using System.Linq;
using System;

namespace PWTransfer.Tests
{
    public class TestMessageStore : MessageStore
    {
        public List<MimeMessage> Messages { get; private set; }
        public readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public TestMessageStore()
        {
            Messages = new List<MimeMessage>();
        }

        public override Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, CancellationToken cancellationToken)
        {
            var textMessage = (ITextMessage)transaction.Message;
            var message = MimeMessage.Load(textMessage.Content);

            Messages.Add(message);
            _semaphore.Release();
            return Task.FromResult(SmtpResponse.Ok);
        }

        public async Task<MimeMessage> WaitForMail()
        {
            var cancelTokenSource = new CancellationTokenSource();
            cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            await _semaphore.WaitAsync(cancelTokenSource.Token);
            return Messages.LastOrDefault();
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

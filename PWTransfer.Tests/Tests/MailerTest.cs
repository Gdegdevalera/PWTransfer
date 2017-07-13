using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PWTransfer.Tests
{
    public class MailerTest
    {
        private readonly TestServer _mailerServer;
        protected readonly HttpClient Mailer;
        private readonly TestMessageStore _messageStore = new TestMessageStore();

        public MailerTest()
        {
            _mailerServer = new TestServer(
               new WebHostBuilder()
                   .UseStartup<Mailer.Startup>());

            Mailer = _mailerServer.CreateClient();

            var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

            var options = new OptionsBuilder()
                .ServerName(configuration["Mailer:Server"])
                .Port(int.Parse(configuration["Mailer:Port"]))
                .MessageStore(_messageStore)
                .AllowUnsecureAuthentication()
                .UserAuthenticator(new SampleUserAuthenticatorFactory())
                .Build();

            var smtpServer = new SmtpServer.SmtpServer(options);
            Task.Run(() => smtpServer.StartAsync(CancellationToken.None));
        }

        [Fact]
        public async Task Send()
        {
            var response = await Mailer.PostFormAsync("/send", new Dictionary<string, string>
            {
                { "email", "laval_alex@mail.ru" },
                { "subject", "test subject" },
                { "body", "test body" },
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, _messageStore.Transactions.Count);
        }
    }
}

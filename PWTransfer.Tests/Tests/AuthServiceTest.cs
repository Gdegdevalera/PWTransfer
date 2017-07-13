using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using SmtpServer;
using Microsoft.Extensions.Configuration;
using SmtpServer.Storage;
using System.Threading;

namespace PWTransfer.Tests
{
    public class AuthServiceTest : TestBase
    {
        private readonly TestMessageStore _messageStore = new TestMessageStore();

        public AuthServiceTest() : base()
        {
            var configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .Build();

            var options = new OptionsBuilder()
                .ServerName(configuration["Mailer:Server"])
                .Port(int.Parse(configuration["Mailer:Port"]))
                .MessageStore(_messageStore)
                .Build();

            var smtpServer = new SmtpServer.SmtpServer(options);
            Task.Run(() => smtpServer.StartAsync(CancellationToken.None));
        }
        
        [Fact]
        public async Task Registration()
        {
            var formData = new Dictionary<string, string>
            {
                { "Name", "testName" },
                { "Email", "email@email.ru" },
                { "Password", "123qwe" },
                { "ConfirmPassword", "123qwe" }
            };

            var response = await Auth.PostFormAsync("/register", formData);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, _messageStore.Transactions.Count());
        }

        [Fact]
        public async Task DoubleRegistration()
        {
            var email = "email@email.ru";

            var formData1 = new Dictionary<string, string>
            {
                { "Name", "testName" },
                { "Email", email },
                { "Password", "123qwe" },
                { "ConfirmPassword", "123qwe" }
            };

            var formData2 = new Dictionary<string, string>
            {
                { "Name", "testName_2" },
                { "Email", email },
                { "Password", "111111" },
                { "ConfirmPassword", "111111" }
            };

            await Auth.PostFormAsync("/register", formData1);

            var response = 
                await Auth.PostFormAsync("/register", formData2);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using SmtpServer;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Text.RegularExpressions;

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
                .AllowUnsecureAuthentication()
                .UserAuthenticator(new SampleUserAuthenticatorFactory())
                .Build();

            var smtpServer = new SmtpServer.SmtpServer(options);
            Task.Run(() => smtpServer.StartAsync(CancellationToken.None));
        }
        
        [Fact]
        public async Task Registration()
        {
            const string email = "register_email@email.ru";
            const string password = "123qwe1";

            var registerFormData = new Dictionary<string, string>
            {
                { "Name", "testName" },
                { "Email", email },
                { "Password", password },
                { "ConfirmPassword", password }
            };

            var registerResponse = await Auth.PostFormAsync("/register", registerFormData);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);     
            
            var loginFormData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password }
            };

            var mail = await _messageStore.WaitForMail();
            Assert.NotNull(mail);

            var tokenExtractor = new Regex(@"confirm\/(.*)['""]");
            var confirmationMessage = mail.Body.ToString();
            var confirmationToken = tokenExtractor.Matches(confirmationMessage)[0].Groups[1].Value;
            Assert.NotEmpty(confirmationToken);

            var confirmFormData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Token", confirmationToken }
            };

            var confirmResponse = await Auth.PostFormAsync("/confirm", confirmFormData);
            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            var loginResponse = await Auth.PostFormAsync("/login", loginFormData);
            var token = await loginResponse.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.NotEmpty(token);
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

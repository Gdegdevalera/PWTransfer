using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PWTransfer.Tests
{
    public class MailerTest
    {
        private readonly TestServer _mailerServer;
        protected readonly HttpClient Mailer;

        public MailerTest()
        {
            _mailerServer = new TestServer(
               new WebHostBuilder()
                   .UseStartup<Mailer.Startup>());

            Mailer = _mailerServer.CreateClient();
        }

        // send real e-mail
        //[Fact]
        public async Task Send()
        {
            var response = await Mailer.PostFormAsync("/send", new Dictionary<string, string>
            {
                { "email", "laval_alex@mail.ru" },
                { "subject", "test subject" },
                { "body", "test body" },
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

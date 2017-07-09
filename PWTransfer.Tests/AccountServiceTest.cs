using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace PWTransfer.Tests
{
    public class AccountServiceTest
    {
        private readonly TestServer _accountServer;
        private readonly HttpClient _account;

        private readonly TestServer _authServer;
        private readonly HttpClient _auth;

        public AccountServiceTest()
        {
            _accountServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AccountService.Startup>());

            _account = _accountServer.CreateClient();

            _authServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AuthService.Startup>());

            _auth = _authServer.CreateClient();
        }

        [Theory]
        [InlineData("GET", "/info")]
        [InlineData("POST", "/create")]
        [InlineData("POST", "/send")]
        public async Task UnauthorizedGet(string method, string route)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), route);
            var response = await _account.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Registration()
        {
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Name", "testName" },
                { "Email", "email@email.ru" },
                { "Password", "123qwe" },
                { "ConfirmPassword", "123qwe" }
            });

            var response = await _auth.PostAsync("/register", formData);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using AccountService;
using System.Threading.Tasks;
using System.Net;

namespace PWTransfer.Tests
{
    public class UnitTest1
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public UnitTest1()
        {
            _server = new TestServer(
                new WebHostBuilder()
                    .UseStartup<Startup>());

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Test1()
        {
            var response = await _client.GetAsync("/info");

            Assert.Equal(response.StatusCode, HttpStatusCode.Unauthorized);
        }
    }
}

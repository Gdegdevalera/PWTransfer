using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace PWTransfer.Tests
{
    public class AccountServiceTest : TestBase
    {
        [Theory]
        [InlineData("GET", "/info")]
        [InlineData("POST", "/create")]
        [InlineData("POST", "/send")]
        public async Task UnauthorizedGet(string method, string route)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), route);
            var response = await Account.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create()
        {
            await AuthorizeAs(TestUser_1);
            var response = await Account.PostAsync("/create", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

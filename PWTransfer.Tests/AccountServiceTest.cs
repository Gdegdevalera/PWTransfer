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

            var accountId = await CreateAccount();
            var info = await Account.GetAsync("/info").Content<AccountInfo>();

            Assert.Equal(accountId, info.UserId);
            Assert.Equal(500, info.Value);
        }

        [Fact]
        public async Task Transfer()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();
            
            var response = await Account.PostAsync("/send", new TransferAction
            {
                Receiver = accountId_1,
                Amount = 10,
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var info_2 = await Account.GetAsync("/info").Content<AccountInfo>();
            Assert.Equal(490, info_2.Value);

            await AuthorizeAs(TestUser_1);
            var info_1 = await Account.GetAsync("/info").Content<AccountInfo>();
            Assert.Equal(510, info_2.Value);
        }
    }

    public class AccountInfo
    {
        public long UserId { get; set; }

        public decimal Value { get; set; }
    }

    public class TransferAction
    {
        public long Receiver { get; set; }

        public decimal Amount { get; set; }
    }
}

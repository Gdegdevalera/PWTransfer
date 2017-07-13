using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using AccountService.Models;

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
            
            var response = await Send(accountId_1, 10m);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(490m, await GetAccountValue());

            await AuthorizeAs(TestUser_1);
            Assert.Equal(510m, await GetAccountValue());
        }

        [Fact]
        public async Task TransferPWCents()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            var response = await Send(accountId_1, 0.01m);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(499.99m, await GetAccountValue());

            await AuthorizeAs(TestUser_1);
            Assert.Equal(500.01m, await GetAccountValue());
        }

        [Fact]
        public async Task TransferVaBanque()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            var value = await GetAccountValue();
            var response = await Send(accountId_1, value);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(0, await GetAccountValue());
        }

        [Fact]
        public async Task TransferMoreThanHave()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            var value = await GetAccountValue();
            var response = await Send(accountId_1, value + 0.01m);
            var afterActionValue = await GetAccountValue();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(value, afterActionValue);
        }

        [Fact]
        public async Task TransferMoreThanHaveByTwoPhases()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            var value = await GetAccountValue();

            await Send(accountId_1, value);
            var response = await Send(accountId_1, 0.01m);
            var afterActionValue = await GetAccountValue();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(0, afterActionValue);
        }

        [Theory]
        [InlineData(1.001)]
        [InlineData(1.009)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task TryTransferInvalid(decimal amount)
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            var value = await GetAccountValue();

            var response = await Send(accountId_1, amount);
            var afterActionValue = await GetAccountValue();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(value, afterActionValue);
        }

        [Fact]
        public async Task TrySendToSelf()
        {
            await AuthorizeAs(TestUser_1);
            var accountId = await CreateAccount();
            var value = await GetAccountValue();

            var response = await Send(accountId, 1);
            var afterActionValue = await GetAccountValue();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(value, afterActionValue);
        }

        [Fact]
        public async Task TrySendToUnknown()
        {
            await AuthorizeAs(TestUser_1);
            var accountId = await CreateAccount();

            var value = await GetAccountValue();

            var response = await Send((UserId)8, 1);
            var afterActionValue = await GetAccountValue();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(value, afterActionValue);
        }

        [Fact]
        public async Task FlushChanges()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            await Send(accountId_1, 1m);
            await Send(accountId_1, 100m);

            await AuthorizeAs(TestUser_1);
            await Send(accountId_2, 10m);

            var flushReport = await Flush();
            Assert.Equal(3, flushReport[accountId_1]);
            Assert.Equal(3, flushReport[accountId_2]);

            Assert.Equal(591m, await GetAccountValue());

            await AuthorizeAs(TestUser_2);
            Assert.Equal(409m, await GetAccountValue());

            Assert.Equal(0, (await Flush()).Count);
        }

        [Fact]
        public async Task FlushTriplet()
        {
            await AuthorizeAs(TestUser_1);
            var accountId_1 = await CreateAccount();

            await AuthorizeAs(TestUser_2);
            var accountId_2 = await CreateAccount();

            await AuthorizeAs(TestUser_3);
            var accountId_3 = await CreateAccount();

            await Send(accountId_1, 1m);

            await AuthorizeAs(TestUser_1);
            await Send(accountId_2, 2m);

            await AuthorizeAs(TestUser_2);
            await Send(accountId_3, 3m);

            var flushResponse = await Flush();
            Assert.Equal(2, flushResponse[accountId_1]);
            Assert.Equal(2, flushResponse[accountId_2]);
            Assert.Equal(2, flushResponse[accountId_3]);

            await AuthorizeAs(TestUser_1);
            Assert.Equal(499m, await GetAccountValue());

            await AuthorizeAs(TestUser_2);
            Assert.Equal(499m, await GetAccountValue());

            await AuthorizeAs(TestUser_3);
            Assert.Equal(502m, await GetAccountValue());
        }
    }
}

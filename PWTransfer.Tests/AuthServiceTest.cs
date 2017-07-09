using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PWTransfer.Tests
{
    public class AuthServiceTest : TestBase
    {
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

            var response = await Auth.PostAsync("/register", formData);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DoubleRegistration()
        {
            const string email = "email@email.ru";

            var formData1 = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Name", "testName" },
                { "Email", email },
                { "Password", "123qwe" },
                { "ConfirmPassword", "123qwe" }
            });

            var formData2 = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Name", "testName_2" },
                { "Email", email },
                { "Password", "111111" },
                { "ConfirmPassword", "111111" }
            });

            await Auth.PostAsync("/register", formData1);

            var response = 
                await Auth.PostAsync("/register", formData2);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}

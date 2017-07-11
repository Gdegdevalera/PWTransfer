using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace PWTransfer.Tests
{
    public class AuthServiceTest : TestBase
    {
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

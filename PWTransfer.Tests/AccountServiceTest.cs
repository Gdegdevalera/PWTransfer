using Xunit;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

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
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            ClearDatabase(configuration.GetConnectionString("AccountService"));
            ClearDatabase(configuration.GetConnectionString("AuthService"));

            _accountServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AccountService.Startup>());

            _account = _accountServer.CreateClient();

            _authServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AuthService.Startup>());

            _auth = _authServer.CreateClient();
        }

        private void ClearDatabase(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    return;
                }

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DECLARE @sql nvarchar(max)

                    DECLARE tableCursor CURSOR FOR
                        WITH tableNames AS (SELECT p.Name FROM sys.objects p
                        INNER JOIN sys.schemas s ON p.[schema_id] = s.[schema_id]
                            WHERE p.[type] = 'U'
                                AND is_ms_shipped = 0
                                AND p.Name not like '_%')
                        SELECT 'TRUNCATE TABLE ' + Name sql FROM tableNames

                    OPEN tableCursor
                    FETCH NEXT FROM tableCursor INTO @sql
                    WHILE @@fetch_status = 0
                    BEGIN
                        PRINT @sql
                        EXEC(@sql)
                        FETCH NEXT FROM tableCursor INTO @sql
                    END
                    CLOSE tableCursor

                    DEALLOCATE tableCursor";
                command.ExecuteNonQuery();
            }
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

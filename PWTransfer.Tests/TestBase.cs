using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Net.Http;

namespace PWTransfer.Tests
{
    public class TestBase
    {
        private readonly TestServer _accountServer;
        protected readonly HttpClient Account;

        private readonly TestServer _authServer;
        protected readonly HttpClient Auth;

        public TestBase()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            ClearDatabase(configuration.GetConnectionString("AccountService"));
            ClearDatabase(configuration.GetConnectionString("AuthService"));

            _accountServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AccountService.Startup>());

            Account = _accountServer.CreateClient();

            _authServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AuthService.Startup>());

            Auth = _authServer.CreateClient();
        }

        private void ClearDatabase(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    // database can be not exists
                    connection.Open();
                }
                catch
                {
                    Console.WriteLine("error");
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
                                AND p.Name <> '__EFMigrationsHistory')
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
    }
}

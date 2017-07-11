using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Scrypt;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PWTransfer.Tests
{
    public class TestBase
    {
        private readonly TestServer _accountServer;
        protected readonly HttpClient Account;

        private readonly TestServer _authServer;
        protected readonly HttpClient Auth;

        private readonly IConfigurationRoot Configuration;

        protected readonly TestUser TestUser_1 = new TestUser
        {
            Name = "Test user 1",
            Password = "13qw2387rt29f",
            Email = "test1@test.test"
        };

        protected readonly TestUser TestUser_2 = new TestUser
        {
            Name = "Test user 2",
            Password = "13qw2387rt29f",
            Email = "test2@test.test"
        };

        public TestBase()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            ClearDatabase(Configuration.GetConnectionString("AccountService"));
            ClearDatabase(Configuration.GetConnectionString("AuthService"));

            _accountServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AccountService.Startup>());

            Account = _accountServer.CreateClient();

            _authServer = new TestServer(
                new WebHostBuilder()
                    .UseStartup<AuthService.Startup>());

            Auth = _authServer.CreateClient();
        }

        protected async Task AuthorizeAs(TestUser user)
        {
            if(user.Token == null)
            {
                GenerateConfirmedUser(user);
                user.Token = await GetToken(user.Email, user.Password);
            }

            Account.DefaultRequestHeaders.Add("Authorization", "Bearer " + user.Token);
        }

        private async Task<string> GetToken(string userEmail, string password)
        {
            var formData = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "Email", userEmail },
                { "Password", password }
            });

            var response = await Auth.PostAsync("/token", formData);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.Content.ReadAsStringAsync();
        }

        private void GenerateConfirmedUser(TestUser user)
        {
            using (var connection = new SqlConnection(Configuration.GetConnectionString("AuthService")))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Users(name, email, passwordhash, state) 
                    VALUES (@name, @email, @password, @state)";
                command.Parameters.Add(new SqlParameter("name", user.Name));
                command.Parameters.Add(new SqlParameter("email", user.Email));
                command.Parameters.Add(new SqlParameter("password", new ScryptEncoder().Encode(user.Password)));
                command.Parameters.Add(new SqlParameter("state", 2)); // Active
                command.ExecuteNonQuery();
            }
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
                    Console.WriteLine("error open DB connection");
                    return;
                }

                var command = connection.CreateCommand();
                command.CommandText = ClearDBScript();
                command.ExecuteNonQuery();
            }
        }

        private string ClearDBScript()
        {
            var assembly = GetType().GetTypeInfo().Assembly;
            var resourceStream = assembly.GetManifestResourceStream("PWTransfer.Tests.ClearDBScript.sql");

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

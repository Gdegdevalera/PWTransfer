using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Service;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;

namespace AuthService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<UserDbContext>(options =>
                      options.UseSqlServer(Configuration.GetConnectionString("AuthService")));

            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(new MailKitOptions()
                {
                    Server = Configuration["Mailer:Server"],
                    Port = int.Parse(Configuration["Mailer:Port"]),
                    SenderName = Configuration["Mailer:SenderName"],
                    SenderEmail = Configuration["Mailer:SenderEmail"],
                    Account = Configuration["Mailer:Account"],
                    Password = Configuration["Mailer:Password"],
                    SSL = bool.Parse(Configuration["Mailer:UseSSL"])
                });
            });

            services.AddSingleton<IJwtGenerator>(new JwtGenerator(Configuration["Auth:SecurityKey"]));
            services.AddSingleton(typeof(IMailService), typeof(MailService));
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<UserDbContext>();
                context.Database.Migrate();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}

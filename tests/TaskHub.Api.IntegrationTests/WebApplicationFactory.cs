using System.Data.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskHub.Api.Infrastructure.Data;
using Xunit;

namespace TaskHub.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove production DbContext options and Npgsql data source
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                var npgsqlDataSourceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(Npgsql.NpgsqlDataSource));
                if (npgsqlDataSourceDescriptor != null)
                {
                    services.Remove(npgsqlDataSourceDescriptor);
                }

                // Add DbContext using the shared connection from DatabaseManager
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(DatabaseManager.Connection);
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Ensure the database schema is created for the shared connection
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
            }

            return host;
        }

        public HttpClient GetAuthenticatedClient(bool isAdmin = false, string? userId = null)
        {
            var client = CreateClient();
            var token = TestDataGenerator.GenerateJwtToken(Services.GetRequiredService<IConfiguration>(), isAdmin, userId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}

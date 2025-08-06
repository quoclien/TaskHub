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
        // Set environment to Testing to avoid runtime seeding
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var applicationDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (applicationDbContextDescriptor != null)
                services.Remove(applicationDbContextDescriptor);

            // Add DbContext using SQLite for testing
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

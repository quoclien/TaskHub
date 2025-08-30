using System.Data.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
                // Remove ALL database-related service registrations more aggressively
                var descriptorsToRemove = new List<ServiceDescriptor>();

                // Find all EF Core related services
                foreach (var service in services.ToList())
                {
                    if (service.ServiceType.IsGenericType)
                    {
                        var genericType = service.ServiceType.GetGenericTypeDefinition();
                        if (genericType == typeof(DbContextOptions<>) ||
                            service.ServiceType.Name.Contains("DbContext"))
                        {
                            descriptorsToRemove.Add(service);
                        }
                    }
                    else if (service.ServiceType == typeof(ApplicationDbContext) ||
                             service.ServiceType == typeof(DbContext) ||
                             service.ServiceType.Name.Contains("DbContext") ||
                             service.ServiceType.Name.Contains("Database"))
                    {
                        descriptorsToRemove.Add(service);
                    }
                }

                // Remove all found services
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add DbContext using SQLite for testing - use a fresh registration
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(DatabaseManager.Connection);
                }, ServiceLifetime.Scoped);

                // Ensure DbContext is available for DI
                services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
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

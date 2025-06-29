using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Infrastructure.Data.Repositories;
using TaskHub.Api.Shared.Mapping;

namespace TaskHub.Api.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add AutoMapper with all profiles in the assembly
            services.AddAutoMapper(typeof(BoardProfile).Assembly);
            
            // Configure MongoDB settings from appsettings.json
            services.Configure<MongoDbSettings>(
                configuration.GetSection(nameof(MongoDbSettings)));

            // Register MongoClient as a singleton
            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(settings.ConnectionString);
            });

            // Register MongoDatabase as a scoped service
            services.AddScoped<IMongoDatabase>(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IMongoClient>();
                var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return client.GetDatabase(settings.DatabaseName);
            });

            // Register repositories
            services.AddScoped<IBoardRepository, BoardRepository>();
            
            return services;
        }
    }
}
}

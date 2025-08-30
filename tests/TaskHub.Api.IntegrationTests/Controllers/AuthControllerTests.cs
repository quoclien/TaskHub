using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Auth;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    [Collection("TestCollection")]
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }



        [Fact]
        public async Task AdminAndDemoUsersSeeded_FromHasData_ShouldExist()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act - Query seeded users
            var adminUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == "admin");
            var demoUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == "demo");

            // Assert - Admin user exists with correct properties
            Assert.NotNull(adminUser);
            Assert.Equal("admin@example.com", adminUser.Email);
            Assert.Equal(UserRole.Admin, adminUser.Role);
            Assert.True(adminUser.IsActive);
            Assert.NotNull(adminUser.PasswordHash);

            // Assert - Demo user exists with correct properties  
            Assert.NotNull(demoUser);
            Assert.Equal("demo@taskhub.com", demoUser.Email);
            Assert.Equal(UserRole.User, demoUser.Role);
            Assert.Equal("Demo", demoUser.FirstName);
            Assert.Equal("User", demoUser.LastName);
            Assert.True(demoUser.IsActive);
            Assert.NotNull(demoUser.PasswordHash);
        }

        [Fact]
        public async Task SeedDataDoesNotInsertDuplicateUsers_OnMultipleRuns()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act - Count users (should be exactly 2 from HasData)
            var userCount = await dbContext.Users.CountAsync();
            var adminCount = await dbContext.Users.CountAsync(u => u.Username == "admin");
            var demoCount = await dbContext.Users.CountAsync(u => u.Username == "demo");

            // Assert - Exactly 2 users total, 1 admin, 1 demo
            Assert.Equal(2, userCount);
            Assert.Equal(1, adminCount);
            Assert.Equal(1, demoCount);
        }

        private async Task SeedAdminUserAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check if admin user already exists
            var existingAdmin = await dbContext.Users.FindAsync(new object[] { "admin" });
            if (existingAdmin != null)
            {
                return; // Admin user already exists
            }

            // Create admin user
            var adminUser = new User
            {
                Id = System.Guid.NewGuid(),
                Username = "admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task Login_WithValidAdminCredentials_ReturnsSuccessAndJwt()
        {
            // Arrange
            await SeedAdminUserAsync();
            var client = _factory.CreateClient();

            var loginDto = new LoginDto
            {
                EmailOrUsername = "admin",
                Password = "admin"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);

            // Verify response contains JWT token
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);
            Assert.NotNull(authResponse.Token);
            Assert.NotEmpty(authResponse.Token);
            Assert.NotNull(authResponse.User);
            Assert.Equal("admin", authResponse.User.Username);
            Assert.Equal("admin@example.com", authResponse.User.Email);
        }

        [Fact]
        public async Task Login_WithAdminEmail_ReturnsSuccessAndJwt()
        {
            // Arrange
            await SeedAdminUserAsync();
            var client = _factory.CreateClient();

            var loginDto = new LoginDto
            {
                EmailOrUsername = "admin@example.com",
                Password = "admin"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);
            Assert.NotNull(authResponse.Token);
            Assert.NotEmpty(authResponse.Token);
            Assert.Equal("admin", authResponse.User.Username);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            await SeedAdminUserAsync();
            var client = _factory.CreateClient();

            var loginDto = new LoginDto
            {
                EmailOrUsername = "admin",
                Password = "wrongpassword"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithNonexistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            var loginDto = new LoginDto
            {
                EmailOrUsername = "nonexistent@example.com",
                Password = "somepassword"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
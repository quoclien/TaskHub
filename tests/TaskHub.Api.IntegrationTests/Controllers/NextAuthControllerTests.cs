using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Controllers;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Users;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    [Collection("TestCollection")]
    public class NextAuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public NextAuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task CleanupUsersAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Remove any existing test users to avoid conflicts
            var testUsers = await dbContext.Users.Where(u => u.Email.Contains("test.oauth")).ToListAsync();
            if (testUsers.Any())
            {
                dbContext.Users.RemoveRange(testUsers);
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GoogleOAuth_WithNewUser_CreatesUserAndReturnsUserDto()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            var googleOAuthRequest = new GoogleOAuthRequest
            {
                Sub = "google_123456789",
                Email = "newuser.test.oauth@example.com",
                Name = "John Doe",
                Image = "https://example.com/avatar.jpg",
                EmailVerified = true
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/nextauth/google-oauth", googleOAuthRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(userDto);
            Assert.Equal(googleOAuthRequest.Email, userDto.Email);
            Assert.Equal("John", userDto.FirstName);
            Assert.Equal("Doe", userDto.LastName);
            Assert.Equal(googleOAuthRequest.Image, userDto.AvatarUrl);

            // Verify user was created in database
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == googleOAuthRequest.Email);
            Assert.NotNull(createdUser);
            Assert.Equal(googleOAuthRequest.Sub, createdUser.GoogleId);
            Assert.True(createdUser.IsActive);
            Assert.Null(createdUser.PasswordHash); // OAuth users don't have password
        }

        [Fact]
        public async Task GoogleOAuth_WithExistingUser_UpdatesUserAndReturnsUserDto()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            
            // Create existing user
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "existing.test.oauth@example.com",
                Username = "existing.test.oauth@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                GoogleId = "google_987654321",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow.AddDays(-1), // Older update time
                LastLoginAt = DateTime.UtcNow.AddDays(-7)
            };
            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();

            var googleOAuthRequest = new GoogleOAuthRequest
            {
                Sub = "google_987654321",
                Email = "existing.test.oauth@example.com",
                Name = "Jane Doe Updated", // Name changed
                Image = "https://example.com/new-avatar.jpg", // Avatar changed
                EmailVerified = true
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/nextauth/google-oauth", googleOAuthRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(userDto);
            Assert.Equal(existingUser.Id, userDto.Id);
            Assert.Equal(googleOAuthRequest.Email, userDto.Email);

            // Verify user was updated in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedUser = await verifyDbContext.Users.FindAsync(existingUser.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("Jane", updatedUser.FirstName);
            Assert.Equal("Doe", updatedUser.LastName); // Last name should be updated
            Assert.Equal(googleOAuthRequest.Image, updatedUser.AvatarUrl);
            Assert.True(updatedUser.UpdatedAt > existingUser.UpdatedAt); // UpdatedAt should be refreshed
            Assert.True(updatedUser.LastLoginAt > existingUser.LastLoginAt); // LastLoginAt should be refreshed
        }

        [Fact]
        public async Task GoogleOAuth_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var googleOAuthRequest = new GoogleOAuthRequest
            {
                Sub = "google_123456789",
                Email = "", // Invalid empty email
                Name = "John Doe",
                EmailVerified = true
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/nextauth/google-oauth", googleOAuthRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidateUser_WithValidActiveUser_ReturnsUserDto()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            
            // Create test user
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "validate.test.oauth@example.com",
                Username = "validate.test.oauth@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.GetAsync($"/api/nextauth/validate-user/{testUser.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(userDto);
            Assert.Equal(testUser.Id, userDto.Id);
            Assert.Equal(testUser.Email, userDto.Email);
            Assert.Equal(testUser.FirstName, userDto.FirstName);
        }

        [Fact]
        public async Task ValidateUser_WithInactiveUser_ReturnsNotFound()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            
            // Create inactive test user
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var inactiveUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive.test.oauth@example.com",
                Username = "inactive.test.oauth@example.com",
                FirstName = "Inactive",
                LastName = "User",
                IsActive = false, // Inactive user
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(inactiveUser);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.GetAsync($"/api/nextauth/validate-user/{inactiveUser.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ValidateUser_WithNonexistentUser_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var nonexistentUserId = Guid.NewGuid();

            // Act
            var response = await client.GetAsync($"/api/nextauth/validate-user/{nonexistentUserId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUserByEmail_WithExistingUser_ReturnsUserDto()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            
            // Create test user
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "getuserbyemail.test.oauth@example.com",
                Username = "getuserbyemail.test.oauth@example.com",
                FirstName = "Email",
                LastName = "Test",
                IsActive = true,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(testUser);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.GetAsync($"/api/nextauth/user-by-email?email={testUser.Email}");

            // Assert
            response.EnsureSuccessStatusCode();
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(userDto);
            Assert.Equal(testUser.Id, userDto.Id);
            Assert.Equal(testUser.Email, userDto.Email);
        }

        [Fact]
        public async Task GetUserByEmail_WithNonexistentEmail_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            var nonexistentEmail = "nonexistent.test.oauth@example.com";

            // Act
            var response = await client.GetAsync($"/api/nextauth/user-by-email?email={nonexistentEmail}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetUserByEmail_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/nextauth/user-by-email?email=");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GoogleOAuth_IdempotencyCheck_SameEmailReturnsConsistentResult()
        {
            // Arrange
            await CleanupUsersAsync();
            var client = _factory.CreateClient();
            var googleOAuthRequest = new GoogleOAuthRequest
            {
                Sub = "google_idempotency_test",
                Email = "idempotency.test.oauth@example.com",
                Name = "Idempotency Test",
                Image = "https://example.com/avatar.jpg",
                EmailVerified = true
            };

            // Act - First call
            var response1 = await client.PostAsJsonAsync("/api/nextauth/google-oauth", googleOAuthRequest);
            var userDto1 = await response1.Content.ReadFromJsonAsync<UserDto>();

            // Act - Second call with same data
            var response2 = await client.PostAsJsonAsync("/api/nextauth/google-oauth", googleOAuthRequest);
            var userDto2 = await response2.Content.ReadFromJsonAsync<UserDto>();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            Assert.NotNull(userDto1);
            Assert.NotNull(userDto2);
            Assert.Equal(userDto1.Id, userDto2.Id); // Should return same user
            Assert.Equal(userDto1.Email, userDto2.Email);

            // Verify only one user was created
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userCount = await dbContext.Users.CountAsync(u => u.Email == googleOAuthRequest.Email);
            Assert.Equal(1, userCount);
        }
    }
}


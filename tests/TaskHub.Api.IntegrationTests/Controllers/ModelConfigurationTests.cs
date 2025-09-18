using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    public class ModelConfigurationTests
    {
        [Fact]
        public void RoleColumnExists_InUserEntity_ModelConfiguration()
        {
            // Arrange - Create DbContext directly with SQLite
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            using var dbContext = new ApplicationDbContext(options);

            // Act & Assert - Check if Role column is properly configured in EF model
            var userEntityType = dbContext.Model.FindEntityType(typeof(User));
            Assert.NotNull(userEntityType);

            var roleProperty = userEntityType.FindProperty(nameof(User.Role));
            Assert.NotNull(roleProperty);
            // Check if Role property exists and is properly configured
            Assert.Equal(typeof(UserRole), roleProperty.ClrType);
            Assert.False(roleProperty.IsNullable); // Should be required
        }

        [Fact]
        public void SeedData_AdminAndDemoUsers_ExistInModel()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;

            using var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureCreated();

            // Act - Check seeded data
            var adminUser = dbContext.Users.FirstOrDefault(u => u.Username == "admin");
            var demoUser = dbContext.Users.FirstOrDefault(u => u.Username == "demo");

            // Assert - Verify seeded users exist
            Assert.NotNull(adminUser);
            Assert.Equal("admin@example.com", adminUser.Email);
            Assert.Equal(UserRole.Admin, adminUser.Role);
            Assert.True(adminUser.IsActive);

            Assert.NotNull(demoUser);
            Assert.Equal("demo@taskhub.com", demoUser.Email);
            Assert.Equal(UserRole.User, demoUser.Role);
            Assert.Equal("Demo", demoUser.FirstName);
            Assert.Equal("User", demoUser.LastName);
            Assert.True(demoUser.IsActive);

            // Verify exactly 2 users (no duplicates)
            var userCount = dbContext.Users.Count();
            Assert.Equal(2, userCount);
        }
    }
}
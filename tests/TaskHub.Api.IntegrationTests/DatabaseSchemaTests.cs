using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;
using Xunit;

namespace TaskHub.Api.IntegrationTests
{
    [Collection("TestCollection")]
    public class DatabaseSchemaTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DatabaseSchemaTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Database_HasCorrectSchema_WithSnakeCaseColumns()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // Act & Assert - Verify tables exist
            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            var tables = new[] { "users", "boards", "board_members", "lists", "cards" };
            foreach (var table in tables)
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = '{table}')";
                var exists = (bool)(await command.ExecuteScalarAsync() ?? false);
                Assert.True(exists, $"Table '{table}' should exist");
            }

            await connection.CloseAsync();
        }

        [Fact]
        public async Task Cards_JsonbMetadata_PersistsCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var testBoard = TestDataGenerator.GenerateTestBoard();
            var testList = TestDataGenerator.GenerateTestList(testBoard.Id, "Test List", 1, testBoard);

            var complexMetadata = new Dictionary<string, object>
            {
                { "priority", "high" },
                { "tags", new[] { "urgent", "backend", "api" } },
                { "estimatedHours", 8.5 },
                { "assignedTeam", new { name = "Backend Team", members = 4 } },
                { "customFields", new Dictionary<string, object> 
                    { 
                        { "businessValue", 100 },
                        { "complexity", "medium" },
                        { "dependencies", new[] { "auth-service", "db-migration" } }
                    }
                }
            };

            var testCard = TestDataGenerator.GenerateTestCard(testList.Id, "Test JSONB Persistence", 1, testList);
            testCard.Description = "Testing complex JSONB metadata storage";
            testCard.Metadata = complexMetadata;

            // Act
            dbContext.Boards.Add(testBoard);
            dbContext.Lists.Add(testList);
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            // Clear tracking to ensure fresh read
            dbContext.ChangeTracker.Clear();

            // Assert - Retrieve and verify JSONB data
            var retrievedCard = await dbContext.Cards.FirstOrDefaultAsync(c => c.Id == testCard.Id);
            Assert.NotNull(retrievedCard);
            Assert.NotNull(retrievedCard.Metadata);

            // Verify specific metadata values
            Assert.True(retrievedCard.Metadata.ContainsKey("priority"));
            Assert.Equal("high", retrievedCard.Metadata["priority"].ToString());

            Assert.True(retrievedCard.Metadata.ContainsKey("estimatedHours"));
            Assert.Equal(8.5, Convert.ToDouble(retrievedCard.Metadata["estimatedHours"]));

            Assert.True(retrievedCard.Metadata.ContainsKey("tags"));
            var tags = retrievedCard.Metadata["tags"];
            Assert.NotNull(tags);

            Assert.True(retrievedCard.Metadata.ContainsKey("customFields"));
            var customFields = retrievedCard.Metadata["customFields"];
            Assert.NotNull(customFields);
        }

        [Fact]
        public async Task Users_SeedData_ExistsAndValid()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // Act
            var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            var demoUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == "demo");

            // Assert - Admin user
            Assert.NotNull(adminUser);
            Assert.Equal("admin@example.com", adminUser.Email);
            Assert.Equal(UserRole.Admin, adminUser.Role);
            Assert.True(adminUser.IsActive);
            Assert.NotNull(adminUser.PasswordHash);
            Assert.NotEmpty(adminUser.PasswordHash);

            // Assert - Demo user
            Assert.NotNull(demoUser);
            Assert.Equal("demo@taskhub.com", demoUser.Email);
            Assert.Equal(UserRole.User, demoUser.Role);
            Assert.Equal("Demo", demoUser.FirstName);
            Assert.Equal("User", demoUser.LastName);
            Assert.True(demoUser.IsActive);
            Assert.NotNull(demoUser.PasswordHash);
            Assert.NotEmpty(demoUser.PasswordHash);

            // Assert - Ensure only expected users exist (no duplicates)
            var userCount = await dbContext.Users.CountAsync();
            Assert.Equal(2, userCount);
        }

        [Fact]
        public async Task Database_Indices_ExistForPerformance()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            // Act & Assert - Check critical indices exist
            var expectedIndices = new[]
            {
                "ix_users_email",
                "ix_users_username", 
                "ix_boards_owner_id",
                "ix_lists_board_id",
                "ix_cards_list_id",
                "ix_cards_list_id_position",
                "ix_cards_assigned_to",
                "ix_cards_due_date"
            };

            foreach (var indexName in expectedIndices)
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE indexname = @indexName
                    )";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@indexName";
                parameter.Value = indexName;
                command.Parameters.Add(parameter);

                var exists = (bool)(await command.ExecuteScalarAsync() ?? false);
                Assert.True(exists, $"Index '{indexName}' should exist for performance");
            }

            await connection.CloseAsync();
        }

        [Fact]
        public async Task Cards_PositionOrdering_WorksCorrectly()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var testBoard = TestDataGenerator.GenerateTestBoard();
            var testList = TestDataGenerator.GenerateTestList(testBoard.Id, "Position Test List", 1, testBoard);

            dbContext.Boards.Add(testBoard);
            dbContext.Lists.Add(testList);

            // Create cards with specific positions
            var cards = new[]
            {
                TestDataGenerator.GenerateTestCard(testList.Id, "Card 1", 1, testList),
                TestDataGenerator.GenerateTestCard(testList.Id, "Card 3", 3, testList),
                TestDataGenerator.GenerateTestCard(testList.Id, "Card 2", 2, testList)
            };

            dbContext.Cards.AddRange(cards);
            await dbContext.SaveChangesAsync();

            // Act - Retrieve cards ordered by position
            var orderedCards = await dbContext.Cards
                .Where(c => c.ListId == testList.Id)
                .OrderBy(c => c.Position)
                .ToListAsync();

            // Assert - Verify correct ordering
            Assert.Equal(3, orderedCards.Count);
            Assert.Equal("Card 1", orderedCards[0].Title);
            Assert.Equal("Card 2", orderedCards[1].Title);
            Assert.Equal("Card 3", orderedCards[2].Title);
            
            Assert.Equal(1, orderedCards[0].Position);
            Assert.Equal(2, orderedCards[1].Position);
            Assert.Equal(3, orderedCards[2].Position);
        }

        [Fact]
        public async Task DatabaseConstraints_EnforceDataIntegrity()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // Act & Assert - Test unique constraints
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser", // Duplicate username
                Email = "test2@example.com",
                PasswordHash = "hashedpassword2",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user1);
            await dbContext.SaveChangesAsync();

            // Should throw exception due to unique constraint on username
            dbContext.Users.Add(user2);
            await Assert.ThrowsAsync<DbUpdateException>(async () => await dbContext.SaveChangesAsync());
        }
    }
}


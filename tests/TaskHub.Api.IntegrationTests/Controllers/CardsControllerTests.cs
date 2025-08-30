using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Cards;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using TaskHub.Api.Models.Entities.Enums;
using Xunit;
using Newtonsoft.Json;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    [Collection("TestCollection")]
    public class CardsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public CardsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<(Board board, List list, Guid userId)> SeedTestBoardWithListAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var testBoard = TestDataGenerator.GenerateTestBoard();
            var boardMember = new BoardMember
            {
                BoardId = testBoard.Id,
                UserId = testBoard.OwnerId,
                Role = BoardRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            var testList = TestDataGenerator.GenerateTestList(testBoard.Id, "Test List", 1, testBoard);

            dbContext.Boards.Add(testBoard);
            dbContext.BoardMembers.Add(boardMember);
            dbContext.Lists.Add(testList);
            await dbContext.SaveChangesAsync();

            return (testBoard, testList, testBoard.OwnerId);
        }

        [Fact]
        public async Task GetCards_WithValidListId_ReturnsCards()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Act
            var response = await client.GetAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards");

            // Assert
            response.EnsureSuccessStatusCode();
            var cards = await response.Content.ReadFromJsonAsync<IEnumerable<CardDto>>();
            Assert.NotNull(cards);
        }

        [Fact]
        public async Task CreateCard_WithValidData_ReturnsCreatedCard()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();
            var createCardDto = new CreateCardDto
            {
                Title = "Test Card",
                Description = "Test Description",
                Position = 1
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards", createCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(createCardDto.Title, result.Title);
            Assert.Equal(createCardDto.Description, result.Description);
            Assert.Equal(list.Id, result.ListId);

            // Verify in database
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdCard = await dbContext.Cards.FindAsync(result.Id);
            Assert.NotNull(createdCard);
            Assert.Equal(createCardDto.Title, createdCard.Title);
        }

        [Fact]
        public async Task CreateCard_WithInvalidListId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _, _) = await SeedTestBoardWithListAsync();
            var invalidListId = Guid.NewGuid();
            var createCardDto = new CreateCardDto
            {
                Title = "Test Card",
                Position = 1
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists/{invalidListId}/cards", createCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCard_WithValidId_ReturnsCard()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Create a test card
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testCard = TestDataGenerator.GenerateTestCard(list.Id, "Test Card", 1, list);
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.GetAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards/{testCard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(testCard.Title, result.Title);
            Assert.Equal(testCard.Id, result.Id);
        }

        [Fact]
        public async Task UpdateCard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Create a test card
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testCard = TestDataGenerator.GenerateTestCard(list.Id, "Original Title", 1, list);
            testCard.Description = "Original Description";
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            var updateCardDto = new UpdateCardDto
            {
                Title = "Updated Title",
                Description = "Updated Description"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards/{testCard.Id}", updateCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedCard = await verifyDbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(updateCardDto.Title, updatedCard.Title);
            Assert.Equal(updateCardDto.Description, updatedCard.Description);
        }

        [Fact]
        public async Task UpdateCard_WithJsonbMetadata_PersistsCorrectly()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Create a test card with initial metadata
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testCard = TestDataGenerator.GenerateTestCard(list.Id, "Test Card", 1, list);
            testCard.Metadata = new Dictionary<string, object>
            {
                { "priority", "high" },
                { "tags", new[] { "urgent", "backend" } }
            };
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            // Update with new metadata
            var newMetadata = new Dictionary<string, object>
            {
                { "priority", "medium" },
                { "tags", new[] { "frontend", "testing" } },
                { "estimatedHours", 8 }
            };

            var updateCardDto = new UpdateCardDto
            {
                Title = "Updated Card"
            };
            updateCardDto.SetMetadataFromDictionary(newMetadata);

            // Act
            var response = await client.PutAsJsonAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards/{testCard.Id}", updateCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify JSONB metadata persistence
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedCard = await verifyDbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(updatedCard);
            Assert.NotNull(updatedCard.Metadata);
            
            // Verify specific metadata values
            Assert.True(updatedCard.Metadata.ContainsKey("priority"));
            Assert.Equal("medium", updatedCard.Metadata["priority"].ToString());
            Assert.True(updatedCard.Metadata.ContainsKey("estimatedHours"));
            Assert.Equal(8, Convert.ToInt32(updatedCard.Metadata["estimatedHours"]));
        }

        [Fact]
        public async Task MoveCard_BetweenLists_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, sourceList, _) = await SeedTestBoardWithListAsync();

            // Create target list
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var targetList = TestDataGenerator.GenerateTestList(board.Id, "Target List", 2, board);
            dbContext.Lists.Add(targetList);

            // Create test card
            var testCard = TestDataGenerator.GenerateTestCard(sourceList.Id, "Test Card", 1, sourceList);
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            var moveCardDto = new MoveCardDto
            {
                TargetListId = targetList.Id,
                NewPosition = 1
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists/{sourceList.Id}/cards/{testCard.Id}/move", moveCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify move in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var movedCard = await verifyDbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(movedCard);
            Assert.Equal(targetList.Id, movedCard.ListId);
            Assert.Equal(moveCardDto.NewPosition, movedCard.Position);
        }

        [Fact]
        public async Task DeleteCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Create a test card
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testCard = TestDataGenerator.GenerateTestCard(list.Id, "Test Card", 1, list);
            dbContext.Cards.Add(testCard);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.DeleteAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards/{testCard.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedCard = await verifyDbContext.Cards.FindAsync(testCard.Id);
            Assert.Null(deletedCard);
        }

        [Fact]
        public async Task CreateCard_VerifiesPositionReordering()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, list, _) = await SeedTestBoardWithListAsync();

            // Create first card
            var firstCardDto = new CreateCardDto { Title = "First Card", Position = 1 };
            await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards", firstCardDto);

            // Create second card that should be inserted at position 1 (pushing first card down)
            var insertedCardDto = new CreateCardDto { Title = "Inserted Card", Position = 1 };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists/{list.Id}/cards", insertedCardDto);

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify position reordering occurred
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cards = dbContext.Cards.Where(c => c.ListId == list.Id).OrderBy(c => c.Position).ToList();
            Assert.Equal(2, cards.Count);
            Assert.Equal("Inserted Card", cards[0].Title);
            Assert.Equal(1, cards[0].Position);
            Assert.Equal("First Card", cards[1].Title);
            Assert.True(cards[1].Position > 1); // Should have been incremented
        }
    }
}
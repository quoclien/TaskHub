using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Dtos.Cards;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using List = TaskHub.Api.Models.Entities.Lists.List;
using Board = TaskHub.Api.Models.Entities.Boards.Board;
using Card = TaskHub.Api.Models.Entities.Cards.Card;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    public class CardsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApplicationDbContext _dbContext;
        private readonly CustomWebApplicationFactory _factory;
        private readonly Board _testBoard;
        private readonly List _testList;
        private bool _disposed;

        public CardsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Create test data
            _testBoard = new Board
            {
                Id = Guid.NewGuid(),
                Title = "Test Board",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OwnerId = Guid.NewGuid()
            };

            _testList = new List
            {
                Id = Guid.NewGuid(),
                Title = "Test List",
                Position = 0,
                BoardId = _testBoard.Id,
                Board = _testBoard,  // Set the navigation property
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // Ensure the board has this list in its collection
            _testBoard.Lists ??= new List<List>();
            _testBoard.Lists.Add(_testList);

            _dbContext.Boards.Add(_testBoard);
            _dbContext.Lists.Add(_testList);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetCards_WithValidListId_ReturnsSuccess()
        {
            // Arrange
            var card = new Card
            {
                Id = Guid.NewGuid(),
                Title = "Test Card",
                Description = "Test Card Description",
                Position = 0,
                ListId = _testList.Id,
                List = _testList, // Set the required navigation property
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7)
            };
            _dbContext.Cards.Add(card);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"api/lists/{_testList.Id}/cards");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardDto[]>();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(card.Title, result[0].Title);
        }

        [Fact]
        public async Task GetCards_ReturnsSuccessStatusCode()
        {
            // Arrange - Add test card
            var testCard = TestDataGenerator.GenerateTestCard(_testList.Id, "Test Card");
            await _dbContext.Cards.AddAsync(testCard);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/lists/{_testList.Id}/cards");

            // Assert
            response.EnsureSuccessStatusCode();
            var cards = await response.Content.ReadFromJsonAsync<List<Card>>();
            Assert.NotNull(cards);
            Assert.Single(cards);
            Assert.Equal("Test Card", cards[0].Title);
        }
        
        [Fact]
        public async Task GetCards_WithNonExistentList_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/lists/{Guid.NewGuid()}/cards");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCard_WithValidId_ReturnsCard()
        {
            // Arrange
            var testCard = TestDataGenerator.GenerateTestCard(_testList.Id, "Test Card");
            await _dbContext.Cards.AddAsync(testCard);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/cards/{testCard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var card = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(card);
            Assert.Equal(testCard.Id, card.Id);
            Assert.Equal("Test Card", card.Title);
            Assert.Equal(_testList.Id, card.ListId);
        }
        
        [Fact]
        public async Task GetCard_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/cards/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateCard_WithValidData_ReturnsCreatedCard()
        {
            // Arrange
            var createCardDto = new CreateCardDto
            {
                Title = "New Test Card",
                Description = "Test Description",
                Position = 0,
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/lists/{_testList.Id}/cards", createCardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdCard = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(createdCard);
            Assert.Equal("New Test Card", createdCard.Title);
            Assert.Equal(_testList.Id, createdCard.ListId);
            Assert.NotNull(createdCard.DueDate);
            Assert.Equal(createCardDto.DueDate.Value.Date, createdCard.DueDate?.Date);
            
            // Verify the card was saved to the database
            var dbCard = await _dbContext.Cards.FindAsync(createdCard.Id);
            Assert.NotNull(dbCard);
            Assert.Equal(createdCard.Title, dbCard.Title);
        }
        
        [Fact]
        public async Task CreateCard_WithEmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var createCardDto = new CreateCardDto
            {
                Title = string.Empty,
                Description = "Test Description",
                Position = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/lists/{_testList.Id}/cards", createCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task CreateCard_WithWhitespaceTitle_ReturnsBadRequest()
        {
            // Arrange
            var createCardDto = new CreateCardDto
            {
                Title = " ",
                Description = "Test Description",
                Position = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/lists/{_testList.Id}/cards", createCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCard_WithValidData_ReturnsCreated()
        {
            // Arrange
            var createCardDto = TestDataGenerator.GenerateCreateCardDto();

            // Act
            var response = await _client.PostAsJsonAsync($"api/lists/{_testList.Id}/cards", createCardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(createCardDto.Title, result.Title);
            Assert.Equal(_testList.Id, result.ListId);
            
            // Verify location header
            Assert.NotNull(response.Headers.Location);
            Assert.Equal($"/api/cards/{result.Id}", response.Headers.Location.LocalPath);
        }

        [Fact]
        public async Task UpdateCard_WithValidData_ReturnsUpdatedCard()
        {
            // Arrange
            var testCard = TestDataGenerator.GenerateTestCard(_testList.Id, "Old Title");
            await _dbContext.Cards.AddAsync(testCard);
            await _dbContext.SaveChangesAsync();

            var updateCardDto = new UpdateCardDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Position = 1,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/cards/{testCard.Id}", updateCardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedCard = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(updatedCard);
            Assert.Equal("Updated Title", updatedCard.Title);
            Assert.Equal("Updated Description", updatedCard.Description);
            Assert.Equal(1, updatedCard.Position);
            Assert.Equal(updateCardDto.DueDate.Value.Date, updatedCard.DueDate?.Date);
            
            // Verify the update was saved to the database
            var dbCard = await _dbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(dbCard);
            Assert.Equal("Updated Title", dbCard.Title);
        }
        
        [Fact]
        public async Task UpdateCard_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var updateCardDto = new UpdateCardDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Position = 1
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/cards/{Guid.NewGuid()}", updateCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateCard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var card = new Card
            {
                Id = Guid.NewGuid(),
                Title = "Test Card",
                Description = "Test Card Description",
                Position = 0,
                ListId = _testList.Id,
                List = _testList, // Set the required navigation property
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7)
            };
            _dbContext.Cards.Add(card);
            await _dbContext.SaveChangesAsync();

            var updateCardDto = new UpdateCardDto 
            { 
                Title = "Updated Card Title",
                Description = "Updated description",
                Position = 1,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            // Act
            var response = await _client.PutAsJsonAsync($"api/cards/{card.Id}", updateCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the card was updated
            var updatedCard = await _dbContext.Cards.FindAsync(card.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(updateCardDto.Title, updatedCard.Title);
            Assert.Equal(updateCardDto.Description, updatedCard.Description);
            Assert.Equal(updateCardDto.Position, updatedCard.Position);
        }

        [Fact]
        public async Task MoveCard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var targetList = TestDataGenerator.GenerateTestList(_testBoard.Id, "Target List");
            _dbContext.Lists.Add(targetList);
            
            var card1 = TestDataGenerator.GenerateTestCard(_testList.Id, "Card 1", 0);
            var card2 = TestDataGenerator.GenerateTestCard(_testList.Id, "Card 2", 1);
            _dbContext.Cards.AddRange(card1, card2);
            await _dbContext.SaveChangesAsync();

            var moveCardDto = new MoveCardDto 
            { 
                TargetListId = targetList.Id,
                NewPosition = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync($"api/cards/{card1.Id}/move", moveCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the card was moved
            var movedCard = await _dbContext.Cards.FindAsync(card1.Id);
            var otherCard = await _dbContext.Cards.FindAsync(card2.Id);
            
            Assert.NotNull(movedCard);
            Assert.NotNull(otherCard);
            Assert.Equal(targetList.Id, movedCard.ListId);
            Assert.Equal(0, movedCard.Position);
            Assert.Equal(0, otherCard.Position); // card2 should move up to position 0
        }

        [Fact]
        public async Task MoveCard_ToDifferentList_UpdatesCardList()
        {
            // Arrange
            var testCard = TestDataGenerator.GenerateTestCard(_testList.Id, "Card to move");
            await _dbContext.Cards.AddAsync(testCard);
            
            var targetList = TestDataGenerator.GenerateTestList(_testBoard.Id, "Target List");
            await _dbContext.Lists.AddAsync(targetList);
            
            await _dbContext.SaveChangesAsync();
            
            var moveCardDto = new MoveCardDto
            {
                TargetListId = targetList.Id,
                NewPosition = 0
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/cards/{testCard.Id}/move", moveCardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedCard = await response.Content.ReadFromJsonAsync<Card>();
            Assert.NotNull(updatedCard);
            Assert.Equal(targetList.Id, updatedCard.ListId);
            Assert.Equal(0, updatedCard.Position);
            
            // Verify the card was moved in the database
            var dbCard = await _dbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(dbCard);
            Assert.Equal(targetList.Id, dbCard.ListId);
            Assert.Equal(0, dbCard.Position);
        }

        [Fact]
        public async Task DeleteCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var testCard = TestDataGenerator.GenerateTestCard(_testList.Id, "Card to delete");
            await _dbContext.Cards.AddAsync(testCard);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/cards/{testCard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verify the card was deleted from the database
            var deletedCard = await _dbContext.Cards.FindAsync(testCard.Id);
            Assert.Null(deletedCard);
        }
        
        [Fact]
        public async Task DeleteCard_WithNonExistentId_ReturnsNotFound()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/cards/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Clean up test data
                _dbContext.Boards.RemoveRange(_dbContext.Boards);
                _dbContext.SaveChanges();
                
                _dbContext.Dispose();
                _client.Dispose();
                
                _disposed = true;
            }
        }
    }
}

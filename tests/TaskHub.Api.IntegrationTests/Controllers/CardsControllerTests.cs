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
using TaskHub.Api.Models.Entities;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    public class CardsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly TrelloDbContext _dbContext;
        private readonly CustomWebApplicationFactory _factory;
        private readonly Board _testBoard;
        private readonly Models.Entities.List _testList;

        public CardsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<TrelloDbContext>();
            
            // Create test data
            _testBoard = TestDataGenerator.GenerateTestBoard();
            _testList = TestDataGenerator.GenerateTestList(_testBoard.Id);
            
            _dbContext.Boards.Add(_testBoard);
            _dbContext.Lists.Add(_testList);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetCards_WithValidListId_ReturnsSuccess()
        {
            // Arrange
            var card = TestDataGenerator.GenerateTestCard(_testList.Id);
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
        public async Task GetCard_WithValidId_ReturnsCard()
        {
            // Arrange
            var card = TestDataGenerator.GenerateTestCard(_testList.Id);
            _dbContext.Cards.Add(card);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"api/cards/{card.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(card.Id, result.Id);
            Assert.Equal(card.Title, result.Title);
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
        public async Task UpdateCard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var card = TestDataGenerator.GenerateTestCard(_testList.Id);
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
        public async Task DeleteCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var card = TestDataGenerator.GenerateTestCard(_testList.Id);
            _dbContext.Cards.Add(card);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"api/cards/{card.Id}?listId={_testList.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the card was deleted
            var deletedCard = await _dbContext.Cards.FindAsync(card.Id);
            Assert.Null(deletedCard);
        }

        public void Dispose()
        {
            // Clean up database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

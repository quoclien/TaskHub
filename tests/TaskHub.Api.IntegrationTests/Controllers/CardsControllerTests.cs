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
using TaskHub.Api.Models.Dtos.Cards;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using Xunit;
using Board = TaskHub.Api.Models.Entities.Boards.Board;
using List = TaskHub.Api.Models.Entities.Lists.List;

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

        private async Task SeedDataAsync(Func<ApplicationDbContext, Task> seeder)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await seeder(dbContext);
        }

        private async Task<(Board, List)> CreateTestBoardAndListAsync(ApplicationDbContext dbContext)
        {
            var board = TestDataGenerator.GenerateTestBoard();
            var list = TestDataGenerator.GenerateTestList(board.Id);
            board.Lists.Add(list);

            dbContext.Boards.Add(board);
            await dbContext.SaveChangesAsync();
            return (board, list);
        }

        [Fact]
        public async Task GetCards_WithValidListId_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList = null;

            await SeedDataAsync(async dbContext =>
            {
                var (_, list) = await CreateTestBoardAndListAsync(dbContext);
                var card = TestDataGenerator.GenerateTestCard(list.Id);
                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();
                testList = list;
            });

            // Act
            var response = await client.GetAsync($"/api/lists/{testList.Id}/cards");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardDto[]>();
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetCard_WithValidId_ReturnsCard()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Card testCard = null;

            await SeedDataAsync(async dbContext =>
            {
                var (_, list) = await CreateTestBoardAndListAsync(dbContext);
                var card = TestDataGenerator.GenerateTestCard(list.Id);
                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();
                testCard = card;
            });

            // Act
            var response = await client.GetAsync($"/api/cards/{testCard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(testCard.Id, result.Id);
        }

        [Fact]
        public async Task CreateCard_WithValidData_ReturnsCreatedCard()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList = null;

            await SeedDataAsync(async dbContext =>
            {
                var (_, list) = await CreateTestBoardAndListAsync(dbContext);
                testList = list;
            });

            var createCardDto = new CreateCardDto { Title = "New Card" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/lists/{testList.Id}/cards", createCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(result);
            Assert.Equal(createCardDto.Title, result.Title);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdCard = await dbContext.Cards.FindAsync(result.Id);
            Assert.NotNull(createdCard);
        }

        [Fact]
        public async Task UpdateCard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Card testCard = null;

            await SeedDataAsync(async dbContext =>
            {
                var (_, list) = await CreateTestBoardAndListAsync(dbContext);
                var card = TestDataGenerator.GenerateTestCard(list.Id);
                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();
                testCard = card;
            });

            var updateCardDto = new UpdateCardDto { Title = "Updated Card" };

            // Act
            var response = await client.PutAsJsonAsync($"/api/cards/{testCard.Id}", updateCardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedCard = await dbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(updatedCard);
            Assert.Equal(updateCardDto.Title, updatedCard.Title);
        }

        [Fact]
        public async Task MoveCard_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Card testCard = null;
            List targetList = null;

            await SeedDataAsync(async dbContext =>
            {
                var (board, list1) = await CreateTestBoardAndListAsync(dbContext);
                var list2 = TestDataGenerator.GenerateTestList(board.Id, "Target List");
                var card = TestDataGenerator.GenerateTestCard(list1.Id);
                dbContext.Lists.Add(list2);
                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();
                testCard = card;
                targetList = list2;
            });

            var moveCardDto = new MoveCardDto
            {
                TargetListId = targetList.Id,
                NewPosition = 0
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/cards/{testCard.Id}/move", moveCardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedCard = await response.Content.ReadFromJsonAsync<CardDto>();
            Assert.NotNull(updatedCard);
            Assert.Equal(targetList.Id, updatedCard.ListId);
            Assert.Equal(0, updatedCard.Position);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbCard = await dbContext.Cards.FindAsync(testCard.Id);
            Assert.NotNull(dbCard);
            Assert.Equal(targetList.Id, dbCard.ListId);
            Assert.Equal(0, dbCard.Position);
        }

        [Fact]
        public async Task DeleteCard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Card testCard = null;

            await SeedDataAsync(async dbContext =>
            {
                var (_, list) = await CreateTestBoardAndListAsync(dbContext);
                var card = TestDataGenerator.GenerateTestCard(list.Id, "Card to delete");
                dbContext.Cards.Add(card);
                await dbContext.SaveChangesAsync();
                testCard = card;
            });

            // Act
            var response = await client.DeleteAsync($"/api/cards/{testCard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedCard = await dbContext.Cards.FindAsync(testCard.Id);
            Assert.Null(deletedCard);
        }

        [Fact]
        public async Task DeleteCard_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();

            // Act
            var response = await client.DeleteAsync($"/api/cards/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

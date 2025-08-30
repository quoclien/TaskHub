using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Boards;
using Board = TaskHub.Api.Models.Entities.Boards.Board;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    [Collection("TestCollection")]
    public class BoardsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public BoardsControllerTests(CustomWebApplicationFactory factory)
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

        [Fact]
        public async Task GetBoards_WithAuthenticatedUser_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();

            // Act
            var response = await client.GetAsync("/api/boards");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task CreateBoard_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var createBoardDto = new CreateBoardDto
            {
                Title = "New Test Board",
                Description = "A board for testing purposes.",
                IsPublic = true
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/boards", createBoardDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<BoardDto>();
            Assert.NotNull(result);
            Assert.Equal(createBoardDto.Title, result.Title);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdBoard = await dbContext.Boards.FindAsync(result.Id);
            Assert.NotNull(createdBoard);
        }

        [Fact]
        public async Task GetBoard_WithValidId_ReturnsBoard()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.GetAsync($"/api/boards/{testBoard.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BoardDto>();
            Assert.NotNull(result);
            Assert.Equal(testBoard.Title, result.Title);
            Assert.Equal(testBoard.IsPublic, result.IsPublic);
        }

        [Fact]
        public async Task GetBoard_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();

            // Act
            var response = await client.GetAsync($"/api/boards/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateBoard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            var updateBoardDto = new UpdateBoardDto
            {
                Title = "Updated Title",
                IsPublic = false
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/boards/{testBoard.Id}", updateBoardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedBoard = await dbContext.Boards.FindAsync(testBoard.Id);
            Assert.NotNull(updatedBoard);
            Assert.Equal(updateBoardDto.Title, updatedBoard.Title);
            Assert.Equal(updateBoardDto.IsPublic, updatedBoard.IsPublic);
        }

        [Fact]
        public async Task DeleteBoard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.DeleteAsync($"/api/boards/{testBoard.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedBoard = await dbContext.Boards.FindAsync(testBoard.Id);
            Assert.Null(deletedBoard);
        }
    }
}

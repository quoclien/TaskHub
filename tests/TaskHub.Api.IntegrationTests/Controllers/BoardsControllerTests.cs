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
    public class BoardsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly TrelloDbContext _dbContext;
        private readonly CustomWebApplicationFactory _factory;

        public BoardsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<TrelloDbContext>();
        }

        [Fact]
        public async Task GetBoards_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/boards");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", 
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetBoard_WithValidId_ReturnsBoard()
        {
            // Arrange
            var board = TestDataGenerator.GenerateTestBoard();
            _dbContext.Boards.Add(board);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/boards/{board.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BoardDto>();
            Assert.NotNull(result);
            Assert.Equal(board.Id, result.Id);
            Assert.Equal(board.Title, result.Title);
        }

        [Fact]
        public async Task GetBoard_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateBoard_WithValidData_ReturnsCreated()
        {
            // Arrange
            var createBoardDto = TestDataGenerator.GenerateCreateBoardDto();

            // Act
            var response = await _client.PostAsJsonAsync("/api/boards", createBoardDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<BoardDto>();
            Assert.NotNull(result);
            Assert.Equal(createBoardDto.Title, result.Title);
            Assert.Equal(createBoardDto.Description, result.Description);
            
            // Verify location header
            Assert.NotNull(response.Headers.Location);
            Assert.Equal($"/api/boards/{result.Id}", response.Headers.Location.LocalPath);
        }

        [Fact]
        public async Task CreateBoard_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createBoardDto = new CreateBoardDto { Title = string.Empty }; // Invalid: Title is required

            // Act
            var response = await _client.PostAsJsonAsync("/api/boards", createBoardDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateBoard_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var board = TestDataGenerator.GenerateTestBoard();
            _dbContext.Boards.Add(board);
            await _dbContext.SaveChangesAsync();

            var updateBoardDto = new UpdateBoardDto 
            { 
                Title = "Updated Title",
                Description = "Updated Description",
                IsPublic = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/boards/{board.Id}", updateBoardDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the board was updated
            var updatedBoard = await _dbContext.Boards.FindAsync(board.Id);
            Assert.NotNull(updatedBoard);
            Assert.Equal(updateBoardDto.Title, updatedBoard.Title);
            Assert.Equal(updateBoardDto.Description, updatedBoard.Description);
            Assert.Equal(updateBoardDto.IsPublic, updatedBoard.IsPublic);
        }

        [Fact]
        public async Task DeleteBoard_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var board = TestDataGenerator.GenerateTestBoard();
            _dbContext.Boards.Add(board);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/boards/{board.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the board was deleted
            var deletedBoard = await _dbContext.Boards.FindAsync(board.Id);
            Assert.Null(deletedBoard);
        }

        public void Dispose()
        {
            // Clean up database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

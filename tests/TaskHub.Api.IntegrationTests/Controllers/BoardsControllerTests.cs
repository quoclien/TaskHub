using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Enums;
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

        [Fact]
        public async Task JoinBoard_PublicBoardAsNonMember_ReturnsSuccessWithJoinedCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                board.IsPublic = true;
                // Remove the auto-added member to simulate non-member joining
                board.BoardMembers.Clear();
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/join", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("joined", result.GetProperty("code").GetString());
            Assert.Equal("Successfully joined board", result.GetProperty("message").GetString());

            // Verify membership was created
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var board = await dbContext.Boards.Include(b => b.BoardMembers).FirstAsync(b => b.Id == testBoard.Id);
            Assert.Single(board.BoardMembers);
            Assert.Equal(BoardRole.Member, board.BoardMembers.First().Role);
        }

        [Fact]
        public async Task JoinBoard_PublicBoardAsExistingMember_ReturnsSuccessWithAlreadyMemberCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                board.IsPublic = true;
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act - joining the board that the user is already a member of (from TestDataGenerator)
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/join", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("already_member", result.GetProperty("code").GetString());
            Assert.Equal("User is already a member of this board", result.GetProperty("message").GetString());
        }

        [Fact]
        public async Task JoinBoard_PrivateBoard_ReturnsForbiddenWithBoardPrivateCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                board.IsPublic = false;
                board.BoardMembers.Clear(); // Remove auto-added member
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/join", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("board_private", result.GetProperty("code").GetString());
            Assert.Equal("Board is private and requires an invitation", result.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task JoinBoard_NonExistentBoard_ReturnsNotFoundWithBoardNotFoundCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var nonExistentBoardId = Guid.NewGuid();

            // Act
            var response = await client.PostAsync($"/api/boards/{nonExistentBoardId}/join", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("board_not_found", result.GetProperty("code").GetString());
            Assert.Equal("Board not found", result.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task LeaveBoard_AsMember_ReturnsSuccessWithLeftCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                // Ensure the user is a member but not the only owner
                var existingMember = board.BoardMembers.First();
                existingMember.Role = BoardRole.Member;

                // Add another owner so the current user can leave
                var anotherOwner = new BoardMember
                {
                    UserId = Guid.NewGuid(),
                    BoardId = board.Id,
                    Role = BoardRole.Owner,
                    JoinedAt = DateTime.UtcNow
                };
                board.BoardMembers.Add(anotherOwner);

                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/leave", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("left", result.GetProperty("code").GetString());
            Assert.Equal("Successfully left board", result.GetProperty("message").GetString());

            // Verify membership was removed
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var board = await dbContext.Boards.Include(b => b.BoardMembers).FirstAsync(b => b.Id == testBoard.Id);
            Assert.Single(board.BoardMembers); // Only the other owner remains
        }

        [Fact]
        public async Task LeaveBoard_AsLastOwner_ReturnsBadRequestWithLastOwnerCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                // Ensure the user is the only owner
                var existingMember = board.BoardMembers.First();
                existingMember.Role = BoardRole.Owner;

                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/leave", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("last_owner", result.GetProperty("code").GetString());
            Assert.Equal("Cannot leave board as the last owner. Transfer ownership or delete the board instead", result.GetProperty("detail").GetString());

            // Verify membership was NOT removed
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var board = await dbContext.Boards.Include(b => b.BoardMembers).FirstAsync(b => b.Id == testBoard.Id);
            Assert.Single(board.BoardMembers); // User is still a member
        }

        [Fact]
        public async Task LeaveBoard_AsNonMember_ReturnsBadRequestWithNotMemberCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                board.BoardMembers.Clear(); // Remove the auto-added member
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.PostAsync($"/api/boards/{testBoard.Id}/leave", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("not_member", result.GetProperty("code").GetString());
            Assert.Equal("User is not a member of this board", result.GetProperty("detail").GetString());
        }

        [Fact]
        public async Task LeaveBoard_NonExistentBoard_ReturnsNotFoundWithBoardNotFoundCode()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var nonExistentBoardId = Guid.NewGuid();

            // Act
            var response = await client.PostAsync($"/api/boards/{nonExistentBoardId}/leave", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("board_not_found", result.GetProperty("code").GetString());
            Assert.Equal("Board not found", result.GetProperty("detail").GetString());
        }
    }
}

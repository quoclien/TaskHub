using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Lists;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Lists;
using TaskHub.Api.Models.Entities.Enums;
using Xunit;

namespace TaskHub.Api.IntegrationTests.Controllers
{
    [Collection("TestCollection")]
    public class ListsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ListsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<(Board board, Guid userId)> SeedTestBoardAsync()
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

            dbContext.Boards.Add(testBoard);
            dbContext.BoardMembers.Add(boardMember);
            await dbContext.SaveChangesAsync();

            return (testBoard, testBoard.OwnerId);
        }

        [Fact]
        public async Task GetLists_WithValidBoardId_ReturnsLists()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();

            // Act
            var response = await client.GetAsync($"/api/boards/{board.Id}/lists");

            // Assert
            response.EnsureSuccessStatusCode();
            var lists = await response.Content.ReadFromJsonAsync<IEnumerable<ListDto>>();
            Assert.NotNull(lists);
        }

        [Fact]
        public async Task CreateList_WithValidData_ReturnsCreatedList()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();
            var createListDto = new CreateListDto
            {
                Title = "Test List",
                Position = 1
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists", createListDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(createListDto.Title, result.Title);
            Assert.Equal(board.Id, result.BoardId);

            // Verify in database
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdList = await dbContext.Lists.FindAsync(result.Id);
            Assert.NotNull(createdList);
            Assert.Equal(createListDto.Title, createdList.Title);
        }

        [Fact]
        public async Task CreateList_WithInvalidBoardId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var invalidBoardId = Guid.NewGuid();
            var createListDto = new CreateListDto
            {
                Title = "Test List",
                Position = 1
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{invalidBoardId}/lists", createListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetList_WithValidId_ReturnsList()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();

            // Create a test list
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testList = TestDataGenerator.GenerateTestList(board.Id, "Test List", 1, board);
            dbContext.Lists.Add(testList);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.GetAsync($"/api/boards/{board.Id}/lists/{testList.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(testList.Title, result.Title);
            Assert.Equal(testList.Id, result.Id);
        }

        [Fact]
        public async Task UpdateList_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();

            // Create a test list
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testList = TestDataGenerator.GenerateTestList(board.Id, "Original Title", 1, board);
            dbContext.Lists.Add(testList);
            await dbContext.SaveChangesAsync();

            var updateListDto = new UpdateListDto
            {
                Title = "Updated Title"
            };

            // Act
            var response = await client.PutAsJsonAsync($"/api/boards/{board.Id}/lists/{testList.Id}", updateListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedList = await verifyDbContext.Lists.FindAsync(testList.Id);
            Assert.NotNull(updatedList);
            Assert.Equal(updateListDto.Title, updatedList.Title);
        }

        [Fact]
        public async Task DeleteList_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();

            // Create a test list
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var testList = TestDataGenerator.GenerateTestList(board.Id, "Test List", 1, board);
            dbContext.Lists.Add(testList);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await client.DeleteAsync($"/api/boards/{board.Id}/lists/{testList.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion in database
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedList = await verifyDbContext.Lists.FindAsync(testList.Id);
            Assert.Null(deletedList);
        }

        [Fact]
        public async Task CreateList_VerifiesPositionAssignment()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            var (board, _) = await SeedTestBoardAsync();

            // Create first list
            var firstListDto = new CreateListDto { Title = "First List", Position = 1 };
            await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists", firstListDto);

            // Create second list with unspecified position
            var secondListDto = new CreateListDto { Title = "Second List", Position = 0 };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{board.Id}/lists", secondListDto);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            
            // Verify position was auto-assigned
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var lists = dbContext.Lists.Where(l => l.BoardId == board.Id).OrderBy(l => l.Position).ToList();
            Assert.Equal(2, lists.Count);
            Assert.True(lists[1].Position > lists[0].Position);
        }
    }
}
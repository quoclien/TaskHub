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
using TaskHub.Api.Models.Dtos.Lists;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Lists;
using List = TaskHub.Api.Models.Entities.Lists.List;
using Board = TaskHub.Api.Models.Entities.Boards.Board;
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

        private async Task SeedDataAsync(Func<ApplicationDbContext, Task> seeder)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await seeder(dbContext);
        }

        [Fact]
        public async Task GetLists_WithValidBoardId_ReturnsSuccess()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            Board testBoard = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                var list = TestDataGenerator.GenerateTestList(board.Id);
                board.Lists.Add(list);
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testBoard = board;
            });

            // Act
            var response = await client.GetAsync($"/api/boards/{testBoard.Id}/lists");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto[]>();
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetList_WithValidId_ReturnsList()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                var list = TestDataGenerator.GenerateTestList(board.Id);
                board.Lists.Add(list);
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testList = list;
            });

            // Act
            var response = await client.GetAsync($"/api/lists/{testList.Id}?boardId={testList.BoardId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(testList.Id, result.Id);
        }

        [Fact]
        public async Task CreateList_WithValidData_ReturnsCreatedList()
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

            var createListDto = new CreateListDto { Title = "New List" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/boards/{testBoard.Id}/lists", createListDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(createListDto.Title, result.Title);

            // Verify the list was created
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdList = await dbContext.Lists.FindAsync(result.Id);
            Assert.NotNull(createdList);
        }

        [Fact]
        public async Task UpdateList_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                var list = TestDataGenerator.GenerateTestList(board.Id);
                board.Lists.Add(list);
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testList = list;
            });

            var updateListDto = new UpdateListDto { Title = "Updated List", Position = 1 };

            // Act
            var response = await client.PutAsJsonAsync($"/api/lists/{testList.Id}", updateListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the list was updated
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedList = await dbContext.Lists.FindAsync(testList.Id);
            Assert.NotNull(updatedList);
            Assert.Equal(updateListDto.Title, updatedList.Title);
            Assert.Equal(updateListDto.Position, updatedList.Position);
        }

        [Fact]
        public async Task DeleteList_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                var list = TestDataGenerator.GenerateTestList(board.Id);
                board.Lists.Add(list);
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testList = list;
            });

            // Act
            var response = await client.DeleteAsync($"/api/lists/{testList.Id}?boardId={testList.BoardId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the list was deleted
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedList = await dbContext.Lists.FindAsync(testList.Id);
            Assert.Null(deletedList);
        }

        [Fact]
        public async Task MoveList_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.GetAuthenticatedClient();
            List testList1 = null;
            List testList2 = null;

            await SeedDataAsync(async dbContext =>
            {
                var board = TestDataGenerator.GenerateTestBoard();
                var list1 = TestDataGenerator.GenerateTestList(board.Id, "List 1", 0);
                var list2 = TestDataGenerator.GenerateTestList(board.Id, "List 2", 1);
                board.Lists.Add(list1);
                board.Lists.Add(list2);
                dbContext.Boards.Add(board);
                await dbContext.SaveChangesAsync();
                testList1 = list1;
                testList2 = list2;
            });

            var updateListDto = new UpdateListDto { Position = 1 };

            // Act
            var response = await client.PutAsJsonAsync($"/api/lists/{testList1.Id}", updateListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the lists were reordered
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedList1 = await dbContext.Lists.FindAsync(testList1.Id);
            var updatedList2 = await dbContext.Lists.FindAsync(testList2.Id);

            Assert.NotNull(updatedList1);
            Assert.NotNull(updatedList2);
            Assert.Equal(1, updatedList1.Position);
            Assert.Equal(0, updatedList2.Position);
        }
    }
}

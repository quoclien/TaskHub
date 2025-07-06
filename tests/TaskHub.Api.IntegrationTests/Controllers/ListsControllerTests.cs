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
    public class ListsControllerTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly TrelloDbContext _dbContext;
        private readonly CustomWebApplicationFactory _factory;
        private readonly Board _testBoard;

        public ListsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var scope = factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<TrelloDbContext>();
            
            // Create a test board for list tests
            _testBoard = TestDataGenerator.GenerateTestBoard();
            _dbContext.Boards.Add(_testBoard);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetLists_WithValidBoardId_ReturnsSuccess()
        {
            // Arrange
            var list = TestDataGenerator.GenerateTestList(_testBoard.Id);
            _dbContext.Lists.Add(list);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/boards/{_testBoard.Id}/lists");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto[]>();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(list.Title, result[0].Title);
        }

        [Fact]
        public async Task GetList_WithValidId_ReturnsList()
        {
            // Arrange
            var list = TestDataGenerator.GenerateTestList(_testBoard.Id);
            _dbContext.Lists.Add(list);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/lists/{list.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(list.Id, result.Id);
            Assert.Equal(list.Title, result.Title);
        }

        [Fact]
        public async Task CreateList_WithValidData_ReturnsCreated()
        {
            // Arrange
            var createListDto = TestDataGenerator.GenerateCreateListDto();

            // Act
            var response = await _client.PostAsJsonAsync($"/api/boards/{_testBoard.Id}/lists", createListDto);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<ListDto>();
            Assert.NotNull(result);
            Assert.Equal(createListDto.Title, result.Title);
            Assert.Equal(_testBoard.Id, result.BoardId);
            
            // Verify location header
            Assert.NotNull(response.Headers.Location);
            Assert.Equal($"/api/lists/{result.Id}", response.Headers.Location.LocalPath);
        }

        [Fact]
        public async Task UpdateList_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var list = TestDataGenerator.GenerateTestList(_testBoard.Id);
            _dbContext.Lists.Add(list);
            await _dbContext.SaveChangesAsync();

            var updateListDto = new UpdateListDto 
            { 
                Title = "Updated List Title",
                Position = 1
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/lists/{list.Id}", updateListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the list was updated
            var updatedList = await _dbContext.Lists.FindAsync(list.Id);
            Assert.NotNull(updatedList);
            Assert.Equal(updateListDto.Title, updatedList.Title);
            Assert.Equal(updateListDto.Position, updatedList.Position);
        }

        [Fact]
        public async Task DeleteList_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var list = TestDataGenerator.GenerateTestList(_testBoard.Id);
            _dbContext.Lists.Add(list);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/lists/{list.Id}?boardId={_testBoard.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the list was deleted
            var deletedList = await _dbContext.Lists.FindAsync(list.Id);
            Assert.Null(deletedList);
        }

        [Fact]
        public async Task MoveList_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var list1 = TestDataGenerator.GenerateTestList(_testBoard.Id, "List 1", 0);
            var list2 = TestDataGenerator.GenerateTestList(_testBoard.Id, "List 2", 1);
            _dbContext.Lists.AddRange(list1, list2);
            await _dbContext.SaveChangesAsync();

            var moveListDto = new MoveListDto { NewPosition = 1 };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/lists/{list1.Id}/move", moveListDto);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the lists were reordered
            var updatedList1 = await _dbContext.Lists.FindAsync(list1.Id);
            var updatedList2 = await _dbContext.Lists.FindAsync(list2.Id);
            
            Assert.NotNull(updatedList1);
            Assert.NotNull(updatedList2);
            Assert.Equal(1, updatedList1.Position);
            Assert.Equal(0, updatedList2.Position);
        }

        public void Dispose()
        {
            // Clean up database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

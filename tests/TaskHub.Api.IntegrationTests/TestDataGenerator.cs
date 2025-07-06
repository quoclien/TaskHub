using System;
using System.Collections.Generic;
using TaskHub.Api.Shared.Models.Dtos;
using TaskHub.Api.Shared.Models.Entities;

namespace TaskHub.Api.IntegrationTests
{
    public static class TestDataGenerator
    {
        public static CreateBoardDto GenerateCreateBoardDto(string title = "Test Board", string description = "Test Description")
        {
            return new CreateBoardDto
            {
                Title = title,
                Description = description,
                IsPublic = true
            };
        }

        public static CreateListDto GenerateCreateListDto(string title = "Test List", int position = 0)
        {
            return new CreateListDto
            {
                Title = title,
                Position = position
            };
        }

        public static CreateCardDto GenerateCreateCardDto(string title = "Test Card", string description = "Test Description", int position = 0)
        {
            return new CreateCardDto
            {
                Title = title,
                Description = description,
                Position = position,
                DueDate = DateTime.UtcNow.AddDays(7)
            };
        }

        public static Board GenerateTestBoard(string title = "Test Board")
        {
            return new Board
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "Test Board Description",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static List GenerateTestList(Guid boardId, string title = "Test List", int position = 0)
        {
            return new List
            {
                Id = Guid.NewGuid(),
                Title = title,
                Position = position,
                BoardId = boardId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Card GenerateTestCard(Guid listId, string title = "Test Card", int position = 0)
        {
            return new Card
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "Test Card Description",
                Position = position,
                ListId = listId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}

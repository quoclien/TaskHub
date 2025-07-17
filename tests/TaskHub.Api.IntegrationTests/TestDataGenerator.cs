using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Dtos.Cards;
using TaskHub.Api.Models.Dtos.Lists;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskHub.Api.Models.Entities.Users;

namespace TaskHub.Api.IntegrationTests
{

    /// <summary>
    /// Helper class for generating test data with properly initialized navigation properties
    /// </summary>
    public static class TestDataGenerator
    {
        public static string GenerateJwtToken(IConfiguration configuration, bool isAdmin = false, string? userId = null)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not found.");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Jwt:Audience not found.");
            var keyString = jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key not found.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId ?? Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iss, issuer),
                new(JwtRegisteredClaimNames.Aud, audience)
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static CreateBoardDto GenerateCreateBoardDto(string title = "Test Board")
        {
            return new CreateBoardDto
            {
                Title = title,
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

        /// <summary>
        /// Creates a test board with all required navigation properties initialized
        /// </summary>
        public static Board GenerateTestBoard(string title = "Test Board", Guid? ownerId = null, bool createOwner = true)
        {
            var boardId = Guid.NewGuid();
            var ownerIdValue = ownerId ?? Guid.NewGuid();
            User? owner = createOwner ? new User 
            { 
                Id = ownerIdValue,
                Username = "testuser",
                Email = "test@example.com",
                IsActive = true
            } : null;
            
            var board = new Board
            {
                Id = boardId,
                Title = title,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OwnerId = ownerIdValue,
                Owner = owner,
                Lists = new List<List>(),
                BoardMembers = new List<BoardMember>()
            };
            
            // Ensure required navigation properties are set
            board.Lists ??= new List<List>();
            board.BoardMembers ??= new List<BoardMember>();
            
            return board;
        }

        /// <summary>
        /// Creates a test list with all required navigation properties initialized
        /// </summary>
        public static List GenerateTestList(Guid? boardId = null, string title = "Test List", int position = 0, Board? board = null)
        {
            var listId = Guid.NewGuid();
            var actualBoard = board ?? GenerateTestBoard(createOwner: true);
            
            // Ensure the board has its collections initialized
            actualBoard.Lists ??= new List<List>();
            
            var list = new List
            {
                Id = listId,
                Title = title,
                Position = position,
                BoardId = actualBoard.Id,
                Board = actualBoard,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Cards = new List<Card>()
            };
            
            // Add the list to the board's collection
            actualBoard.Lists.Add(list);
            
            return list;
        }

        /// <summary>
        /// Creates a test card with all required navigation properties initialized
        /// </summary>
        public static Card GenerateTestCard(Guid? listId = null, string title = "Test Card", int position = 0, List? list = null)
        {
            var cardId = Guid.NewGuid();
            var actualList = list ?? GenerateTestList();
            
            // Ensure the list has its cards collection initialized
            actualList.Cards ??= new List<Card>();
            
            var card = new Card
            {
                Id = cardId,
                Title = title,
                Description = "Test Card Description",
                Position = position,
                ListId = actualList.Id,
                List = actualList,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                IsArchived = false
            };
            
            // Add the card to the list's collection
            actualList.Cards.Add(card);
            
            return card;
        }
    }
}

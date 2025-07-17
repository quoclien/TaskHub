using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Lists;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Enums;
using BCrypt.Net;

namespace TaskHub.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                await context.Database.MigrateAsync();
            }

            await SeedDataAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping seed data creation.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // Create demo user
        var demoUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "demo",
            Email = "demo@taskhub.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            FirstName = "Demo",
            LastName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(demoUser);
        await context.SaveChangesAsync();

        // Create demo board
        var demoBoard = new Board
        {
            Id = Guid.NewGuid(),
            Title = "Demo Project Board",
            Description = "A sample board to demonstrate TaskHub features",
            IsPublic = false,
            IsArchived = false,
            OwnerId = demoUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Boards.Add(demoBoard);

        // Add board member
        var boardMember = new BoardMember
        {
            BoardId = demoBoard.Id,
            UserId = demoUser.Id,
            Role = BoardRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        context.BoardMembers.Add(boardMember);
        await context.SaveChangesAsync();

        // Create demo lists
        var todoList = new List
        {
            Id = Guid.NewGuid(),
            Title = "To Do",
            BoardId = demoBoard.Id,
            Position = 1,
            Board = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var inProgressList = new List
        {
            Id = Guid.NewGuid(),
            Title = "In Progress",
            BoardId = demoBoard.Id,
            Position = 2,
            Board = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var doneList = new List
        {
            Id = Guid.NewGuid(),
            Title = "Done",
            BoardId = demoBoard.Id,
            Position = 3,
            Board = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Lists.AddRange(todoList, inProgressList, doneList);
        await context.SaveChangesAsync();

        // Create demo cards
        var welcomeCard = new Card
        {
            Id = Guid.NewGuid(),
            Title = "Welcome to TaskHub!",
            Description = "This is a sample card demonstrating the TaskHub interface. You can create, edit, and move cards between lists.",
            ListId = todoList.Id,
            Position = 1,
            IsArchived = false,
            List = null!,
            Metadata = new Dictionary<string, object>
            {
                { "tags", new[] { "welcome", "demo" } },
                { "priority", "high" },
                { "color", "#3b82f6" }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var setupCard = new Card
        {
            Id = Guid.NewGuid(),
            Title = "Set up development environment",
            Description = "Configure local development environment for the TaskHub application",
            ListId = inProgressList.Id,
            Position = 1,
            AssignedTo = demoUser.Id,
            DueDate = DateTime.UtcNow.AddDays(7),
            IsArchived = false,
            List = null!,
            Metadata = new Dictionary<string, object>
            {
                { "tags", new[] { "setup", "development" } },
                { "priority", "medium" },
                { "estimatedHours", 4 }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var docsCard = new Card
        {
            Id = Guid.NewGuid(),
            Title = "Review project documentation",
            Description = "Read through the project README and API documentation",
            ListId = doneList.Id,
            Position = 1,
            AssignedTo = demoUser.Id,
            IsArchived = false,
            List = null!,
            Metadata = new Dictionary<string, object>
            {
                { "tags", new[] { "documentation", "onboarding" } },
                { "priority", "low" },
                { "completedAt", DateTime.UtcNow.AddDays(-1).ToString("O") }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Cards.AddRange(welcomeCard, setupCard, docsCard);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with demo data");
    }
} 
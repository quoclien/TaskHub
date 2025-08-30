using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using TaskHub.Api.Models.Entities.Users;
using TaskHub.Api.Models.Entities.Enums;
using BCrypt.Net;

namespace TaskHub.Api.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardMember> BoardMembers { get; set; }
        public DbSet<List> Lists { get; set; }
        public DbSet<Card> Cards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUser(modelBuilder);
            ConfigureBoard(modelBuilder);
            ConfigureBoardMember(modelBuilder);
            ConfigureList(modelBuilder);
            ConfigureCard(modelBuilder);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(50);

                entity.Property(e => e.AvatarUrl)
                    .HasColumnName("avatar_url")
                    .HasMaxLength(500);

                entity.Property(e => e.GoogleId)
                    .HasColumnName("google_id")
                    .HasMaxLength(100);

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(true);

                entity.Property(e => e.LastLoginAt)
                    .HasColumnName("last_login_at");

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .IsRequired()
                    .HasDefaultValue(UserRole.User)
                    .HasSentinel((UserRole)0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                // Indexes
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("ix_users_email");

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("ix_users_username");

                // Seed data with static values  
                entity.HasData(
                    new User
                    {
                        Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash = "$2a$11$rLmZgDJGNuFu.M7P1zYL1uZ7h4YDZE4XlkV3mL3LzHBE4jQ7jWf1m", // admin
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime(),
                        UpdatedAt = DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime()
                    },
                    new User
                    {
                        Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                        Username = "demo",
                        Email = "demo@taskhub.com",
                        PasswordHash = "$2a$11$8K2wJZjAe3qXV3xkR1L9K.6D4E0.aGJo.dI/MgJ8X.5sT9rYzN7mO", // Demo123!
                        Role = UserRole.User,
                        FirstName = "Demo",
                        LastName = "User",
                        IsActive = true,
                        CreatedAt = DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime(),
                        UpdatedAt = DateTime.Parse("2024-01-01T00:00:00Z").ToUniversalTime()
                    }
                );
            });
        }

        private void ConfigureBoard(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>(entity =>
            {
                entity.ToTable("boards");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.IsPublic)
                    .HasColumnName("is_public")
                    .HasDefaultValue(false);

                entity.Property(e => e.IsArchived)
                    .HasColumnName("is_archived")
                    .HasDefaultValue(false);

                entity.Property(e => e.OwnerId)
                    .HasColumnName("owner_id")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                // Relationships
                entity.HasOne(b => b.Owner)
                    .WithMany(u => u.OwnedBoards)
                    .HasForeignKey(b => b.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.OwnerId)
                    .HasDatabaseName("ix_boards_owner_id");

                entity.HasIndex(e => e.IsPublic)
                    .HasDatabaseName("ix_boards_is_public");
            });
        }

        private void ConfigureBoardMember(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoardMember>(entity =>
            {
                entity.ToTable("board_members");
                entity.HasKey(bm => new { bm.BoardId, bm.UserId });

                entity.Property(e => e.BoardId)
                    .HasColumnName("board_id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .IsRequired();

                entity.Property(e => e.JoinedAt)
                    .HasColumnName("joined_at");

                // Relationships
                entity.HasOne(bm => bm.Board)
                    .WithMany(b => b.BoardMembers)
                    .HasForeignKey(bm => bm.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bm => bm.User)
                    .WithMany(u => u.BoardMemberships)
                    .HasForeignKey(bm => bm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.BoardId)
                    .HasDatabaseName("ix_board_members_board_id");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("ix_board_members_user_id");
            });
        }

        private void ConfigureList(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<List>(entity =>
            {
                entity.ToTable("lists");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.BoardId)
                    .HasColumnName("board_id")
                    .IsRequired();

                entity.Property(e => e.Position)
                    .HasColumnName("position")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                // Relationships
                entity.HasOne(l => l.Board)
                    .WithMany(b => b.Lists)
                    .HasForeignKey(l => l.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.BoardId)
                    .HasDatabaseName("ix_lists_board_id");

                entity.HasIndex(e => new { e.BoardId, e.Position })
                    .HasDatabaseName("ix_lists_board_id_position");
            });
        }

        private void ConfigureCard(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Card>(entity =>
            {
                entity.ToTable("cards");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000);

                entity.Property(e => e.ListId)
                    .HasColumnName("list_id")
                    .IsRequired();

                entity.Property(e => e.Position)
                    .HasColumnName("position")
                    .IsRequired();

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date");

                entity.Property(e => e.AssignedTo)
                    .HasColumnName("assigned_to");

                entity.Property(e => e.IsArchived)
                    .HasColumnName("is_archived")
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                // Configure JSONB metadata column
                entity.Property(e => e.Metadata)
                    .HasColumnName("metadata")
                    .HasColumnType("jsonb");

                // Relationships
                entity.HasOne(c => c.List)
                    .WithMany(l => l.Cards)
                    .HasForeignKey(c => c.ListId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.ListId)
                    .HasDatabaseName("ix_cards_list_id");

                entity.HasIndex(e => new { e.ListId, e.Position })
                    .HasDatabaseName("ix_cards_list_id_position");

                entity.HasIndex(e => e.AssignedTo)
                    .HasDatabaseName("ix_cards_assigned_to");

                entity.HasIndex(e => e.DueDate)
                    .HasDatabaseName("ix_cards_due_date");
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Cards;
using TaskHub.Api.Models.Entities.Lists;
using TaskHub.Api.Models.Entities.Users;

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

            modelBuilder.Entity<Board>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.OwnedBoards)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BoardMember>()
                .HasKey(bm => new { bm.BoardId, bm.UserId });

            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.Board)
                .WithMany(b => b.BoardMembers)
                .HasForeignKey(bm => bm.BoardId);

            modelBuilder.Entity<BoardMember>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.BoardMemberships)
                .HasForeignKey(bm => bm.UserId);
        }
    }
}

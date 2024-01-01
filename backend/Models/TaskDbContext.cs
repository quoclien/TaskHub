using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.Driver;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    // Define your entities as DbSet properties
    public DbSet<Task> Tasks { get; init; }
    public static TaskDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<TaskDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Task>().ToCollection("tasks");
    }
}
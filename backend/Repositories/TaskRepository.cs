using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class TaskRepository: ITaskRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task? GetTaskById(string id)
    {
        return _dbContext.Tasks.FirstOrDefault(t => t.Id == id);
    }

    public List<Task> GetAllTasks()
    {
        return _dbContext.Tasks.ToList();
    }

    public void AddTask(Task task)
    {
        _dbContext.Tasks.Add(task);
        _dbContext.SaveChanges();
    }

    public void UpdateTask(Task task)
    {
        _dbContext.Entry(task).State = EntityState.Modified;
        _dbContext.SaveChanges();
    }

    public void DeleteTask(Task task)
    {
        _dbContext.Tasks.Remove(task);
        _dbContext.SaveChanges();
    }
}
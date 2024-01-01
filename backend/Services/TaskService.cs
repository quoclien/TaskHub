using System.Collections.Generic;

public class TaskService: ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public Task? GetTaskById(string id)
    {
        return _taskRepository.GetTaskById(id);
    }

    public List<Task> GetAllTasks()
    {
        return _taskRepository.GetAllTasks();
    }

    public void CreateTask(Task task)
    {
        _taskRepository.AddTask(task);
    }

    public void UpdateTask(Task task)
    {
        _taskRepository.UpdateTask(task);
    }

    public void DeleteTask(string id)
    {
        Task? task = _taskRepository.GetTaskById(id);
        if (task != null)
        {
            _taskRepository.DeleteTask(task);
        }
    }
}
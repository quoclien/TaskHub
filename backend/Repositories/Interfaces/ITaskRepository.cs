public interface ITaskRepository
{
    Task? GetTaskById(string id);
    List<Task> GetAllTasks();
    void AddTask(Task task);
    void UpdateTask(Task task);
    void DeleteTask(Task task);
}
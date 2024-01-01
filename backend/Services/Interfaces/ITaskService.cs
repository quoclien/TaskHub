public interface ITaskService
{
    Task? GetTaskById(string id);
    List<Task> GetAllTasks();
    void CreateTask(Task task);
    void UpdateTask(Task task);
    void DeleteTask(string id);
}
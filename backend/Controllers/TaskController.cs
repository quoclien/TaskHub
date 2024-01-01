using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public IActionResult GetAllTasks()
    {
        var tasks = _taskService.GetAllTasks();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public IActionResult GetTaskById(string id)
    {
        var task = _taskService.GetTaskById(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost]
    public IActionResult CreateTask(Task task)
    {
        _taskService.CreateTask(task);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateTask(string id, Task task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        _taskService.UpdateTask(task);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTask(string id)
    {
        _taskService.DeleteTask(id);
        return NoContent();
    }
}
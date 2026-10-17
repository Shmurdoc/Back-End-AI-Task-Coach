using Application.DTOs.TaskDtos;
using Application.IService;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Filters;
using WebAPI.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnsureSelfUser]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly INotificationBroadcaster _broadcaster;
    public TasksController(ITaskService taskService, INotificationBroadcaster broadcaster)
    {
        _taskService = taskService;
        _broadcaster = broadcaster;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var t = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        if (t == null) return NotFound(new { success = false, error = "Task not found" });
        return Ok(new { success = true, data = t });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId, CancellationToken cancellationToken)
        => Ok(new { success = true, data = await _taskService.GetUserTasksAsync(userId, cancellationToken) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateTaskAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _taskService.UpdateTaskAsync(id, dto, cancellationToken);
            if (dto.Status == TaskItemStatus.Completed)
                await _broadcaster.TaskCompletedAsync(updated.UserId, updated, cancellationToken);
            return Ok(new { success = true, data = updated });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { success = false, error = "Task not found" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _taskService.DeleteTaskAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, error = "Task not found" });
        return Ok(new { success = true });
    }
}

using Application.CQRS.Commands.Tasks;
using Application.CQRS.Queries.Tasks;
using Application.DTOs.TaskDtos;
using Application.IService;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAdaptiveSchedulingEngine _scheduler;
    public TasksController(IMediator mediator, IAdaptiveSchedulingEngine scheduler) { _mediator = mediator; _scheduler = scheduler; }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var t = await _mediator.Send(new GetTaskByIdQuery(id));
        if (t == null) return NotFound(new { success = false, error = "Task not found" });
        return Ok(new { success = true, data = t });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId)
        => Ok(new { success = true, data = await _mediator.Send(new GetUserTasksQuery(userId)) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(new CreateTaskCommand(dto), cancellationToken);
        _ = _scheduler.RescheduleAsync(created.UserId);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new UpdateTaskCommand(id, dto), cancellationToken);
        _ = _scheduler.RescheduleAsync(updated.UserId);
        return Ok(new { success = true, data = updated });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTaskCommand(id), cancellationToken);
        return Ok(new { success = true });
    }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Increment the nudges_delivered metric as a placeholder for test
            WebAPI.Extensions.ObservabilityExtensions.NudgesDelivered.Add(1);
            return Ok(new { success = true, message = "Tasks API root." });
        }
}


using Application.CQRS.Commands.Goals;
using Application.CQRS.Queries.Goals;
using Application.DTOs.GoalDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public GoalsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var g = await _mediator.Send(new GetGoalByIdQuery(id));
        if (g is null) return NotFound(new { success = false, error = "Goal not found" });
        return Ok(new { success = true, data = g });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId)
        => Ok(new { success = true, data = await _mediator.Send(new GetUserGoalsQuery(userId)) });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(new CreateGoalCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoalDto dto, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new UpdateGoalCommand(id, dto), cancellationToken);
        return Ok(new { success = true, data = updated });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteGoalCommand(id), cancellationToken);
        return Ok(new { success = true });
    }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            WebAPI.Extensions.ObservabilityExtensions.NudgesDelivered.Add(1);
            return Ok(new { success = true, message = "Goals API root." });
        }
}

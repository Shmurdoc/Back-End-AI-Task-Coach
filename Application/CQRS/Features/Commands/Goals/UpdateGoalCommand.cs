using Application.DTOs.GoalDtos;
using MediatR;

namespace Application.CQRS.Features.Commands.Goals
{
    // UpdateGoalCommand.cs
    public record UpdateGoalCommand(Guid Id, UpdateGoalDto Dto) : IRequest<GoalDto>;
}

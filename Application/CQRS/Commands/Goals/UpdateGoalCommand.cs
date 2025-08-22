using Application.DTOs.GoalDtos;
using MediatR;

namespace Application.CQRS.Commands.Goals;

public record UpdateGoalCommand(Guid GoalId, UpdateGoalDto Dto) : IRequest<GoalDto>;

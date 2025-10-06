using Application.DTOs.GoalDtos;
using MediatR;

namespace Application.CQRS.Commands.Goals;

public record CreateGoalCommand(CreateGoalDto Dto) : IRequest<GoalDto>;

using Application.DTOs.GoalDtos;
using MediatR;

namespace Application.CQRS.Queries.Goals;

public record GetGoalByIdQuery(Guid GoalId) : IRequest<GoalDto?>;

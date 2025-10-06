using Application.DTOs.GoalDtos;
using MediatR;

namespace Application.CQRS.Queries.Goals;

public record GetUserGoalsQuery(Guid UserId) : IRequest<IEnumerable<GoalDto>>;

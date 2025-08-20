using Application.DTOs.GoalDtos;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Goals;

public record GetUserGoalsQuery(Guid UserId) : IRequest<IEnumerable<GoalDto>>;

// No changes needed


using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Goals;

public record GetGoalByIdQuery(Guid GoalId, Guid UserId) : IRequest<Goal?>;

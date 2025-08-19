using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Goals;

public record GetUserGoalsQuery(Guid UserId) : IRequest<IEnumerable<Goal>>;

// No changes needed


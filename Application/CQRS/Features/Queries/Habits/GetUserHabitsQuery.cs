using MediatR;
using Domain.Entities;

namespace Application.CQRS.Features.Queries.Habits;

public record GetUserHabitsQuery(Guid UserId) : IRequest<IEnumerable<Habit>>;

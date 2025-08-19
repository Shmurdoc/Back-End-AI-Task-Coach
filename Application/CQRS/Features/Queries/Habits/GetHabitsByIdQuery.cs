using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Habits;

public record GetHabitsByIdQuery(Guid HabitId, Guid UserId) : IRequest<Habit?>;

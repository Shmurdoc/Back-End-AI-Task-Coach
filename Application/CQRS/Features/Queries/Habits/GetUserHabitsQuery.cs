using MediatR;
using Domain.Entities;
using Application.DTOs.HabitDtos;

namespace Application.CQRS.Features.Queries.Habits;

public record GetUserHabitsQuery(Guid UserId) : IRequest<IEnumerable<HabitDto>>;

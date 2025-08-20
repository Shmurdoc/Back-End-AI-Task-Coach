using Application.DTOs.HabitDtos;
using Domain.Entities;
using MediatR;

namespace Application.CQRS.Features.Queries.Habits
{
    public record ListHabitsQuery : IRequest<List<HabitDto>>;
}

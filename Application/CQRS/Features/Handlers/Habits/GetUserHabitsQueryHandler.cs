using MediatR;
using Application.CQRS.Features.Queries.Habits;
using Application.IRepositories;
using Domain.Entities;
using Application.DTOs.HabitDtos;
using Application.IService;

namespace Application.CQRS.Features.Handlers.Habits;

public class GetUserHabitsQueryHandler : IRequestHandler<GetUserHabitsQuery, IEnumerable<HabitDto>>
{
    private readonly IHabitService _habitService;

    public GetUserHabitsQueryHandler(IHabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<IEnumerable<HabitDto>> Handle(GetUserHabitsQuery request, CancellationToken cancellationToken)
    {
        return await _habitService.GetUserHabitsAsync(request.UserId);
    }
}

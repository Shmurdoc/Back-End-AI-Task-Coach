using MediatR;
using Application.CQRS.Features.Queries.Habits;
using Application.IRepositories;
using Domain.Entities;

namespace Application.CQRS.Features.Handlers.Habits;

public class GetUserHabitsQueryHandler : IRequestHandler<GetUserHabitsQuery, IEnumerable<Habit>>
{
    private readonly IHabitRepository _habitRepository;

    public GetUserHabitsQueryHandler(IHabitRepository habitRepository)
    {
        _habitRepository = habitRepository;
    }

    public async Task<IEnumerable<Habit>> Handle(GetUserHabitsQuery request, CancellationToken cancellationToken)
    {
        return await _habitRepository.GetUserHabitsAsync(request.UserId);
    }
}

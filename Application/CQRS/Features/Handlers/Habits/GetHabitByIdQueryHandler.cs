using Application.CQRS.Features.Queries.Habits;
using Application.DTOs.HabitDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Habits;

public class GetHabitByIdQueryHandler : IRequestHandler<GetHabitsByIdQuery, HabitDto?>
{
    private readonly IHabitService _habitService;

    public GetHabitByIdQueryHandler(IHabitService habitService)
    {
        _habitService = habitService;
    }


    public Task<HabitDto?> Handle(GetHabitsByIdQuery request, CancellationToken cancellationToken)
    {
        return _habitService.GetHabitByIdAsync(request.HabitId, cancellationToken);
    }
}

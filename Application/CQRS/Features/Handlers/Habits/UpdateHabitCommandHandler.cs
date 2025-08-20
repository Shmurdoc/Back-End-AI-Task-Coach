using Application.CQRS.Features.Commands.Habits;
using Application.DTOs.HabitDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Habits{

public class UpdateHabitCommandHandler : IRequestHandler<UpdateHabitCommand, HabitDto>
{
    private readonly IHabitService _habitService;

    public UpdateHabitCommandHandler(IHabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<HabitDto> Handle(UpdateHabitCommand request, CancellationToken cancellationToken)
    {
        return await _habitService.UpdateHabitAsync(request.Id, request.Dto, cancellationToken);
    }
}
}
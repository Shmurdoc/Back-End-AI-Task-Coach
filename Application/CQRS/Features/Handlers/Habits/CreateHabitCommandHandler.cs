using MediatR;
using Domain.Entities;
using Application.CQRS.Features.Commands.Habits;
using Application.DTOs.HabitDtos;
using Application.IService;

namespace Application.CQRS.Features.Handlers.Habits;

public class CreateHabitCommandHandler : IRequestHandler<CreateHabitCommand, HabitDto>
{
    private readonly IHabitService _habitService;

    public CreateHabitCommandHandler(IHabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<HabitDto> Handle(CreateHabitCommand request, CancellationToken cancellationToken)
    {
        return await _habitService.CreateHabitAsync(request.CreateHabitDto, cancellationToken) ?? null!;
    }
}




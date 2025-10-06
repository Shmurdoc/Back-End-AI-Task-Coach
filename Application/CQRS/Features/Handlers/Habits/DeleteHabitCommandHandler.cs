using Application.CQRS.Features.Commands.Habits;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Habits
{

public class DeleteHabitCommandHandler : IRequestHandler<DeleteHabitCommand, bool>
{
    private readonly IHabitService _habitService;

    public DeleteHabitCommandHandler(IHabitService habitService)
    {
        _habitService = habitService;
    }

    public async Task<bool> Handle(DeleteHabitCommand request, CancellationToken cancellationToken)
    {
        return await _habitService.DeleteHabitAsync(request.Id, cancellationToken);
    }
}

}

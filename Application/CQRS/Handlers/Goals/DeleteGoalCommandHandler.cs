using Application.CQRS.Commands.Goals;
using Application.IRepositories;
using MediatR;

namespace Application.CQRS.Handlers.Goals;

public class DeleteGoalCommandHandler : IRequestHandler<DeleteGoalCommand, bool>
{
    private readonly IGoalRepository _goalRepository;
    public DeleteGoalCommandHandler(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task<bool> Handle(DeleteGoalCommand request, CancellationToken cancellationToken)
    {
        await _goalRepository.DeleteAsync(request.GoalId);
        return true;
    }
}

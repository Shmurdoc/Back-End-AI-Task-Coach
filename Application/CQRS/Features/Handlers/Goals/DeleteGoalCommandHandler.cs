using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CQRS.Features.Commands.Goals;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class DeleteGoalCommandHandler : IRequestHandler<DeleteGoalCommand, bool>
{
    private readonly IGoalService _goalService;

    public DeleteGoalCommandHandler(IGoalService goalService)
    {
        _goalService = goalService;
    }

    public async Task<bool> Handle(DeleteGoalCommand request, CancellationToken cancellationToken)
    {
        return await _goalService.DeleteGoalAsync(request.Id, cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CQRS.Features.Commands.Goals;
using Application.DTOs.GoalDtos;
using Application.IService;
using MediatR;

namespace Application.CQRS.Features.Handlers.Goals;

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, GoalDto>
{
    private readonly IGoalService _goalService;

    public UpdateGoalCommandHandler(IGoalService goalService)
    {
        _goalService = goalService;
    }

    public async Task<GoalDto> Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        return await _goalService.UpdateGoalAsync(request.Id, request.Dto, cancellationToken);
    }
}

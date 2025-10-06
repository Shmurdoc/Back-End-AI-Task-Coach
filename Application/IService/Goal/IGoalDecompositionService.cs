using Application.DTOs.GoalDtos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.IService.Goal
{
    public interface IGoalDecompositionService
    {
        Task<GoalDecompositionDto> DecomposeGoalAsync(Guid goalId, string goalTitle, CancellationToken ct = default);
    }
}

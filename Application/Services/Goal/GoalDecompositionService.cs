using Application.DTOs.GoalDtos;
using Application.IService.Goal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Goal
{
    public class GoalDecompositionService : IGoalDecompositionService
    {
        public async Task<GoalDecompositionDto> DecomposeGoalAsync(Guid goalId, string goalTitle, CancellationToken ct = default)
        {
            // TODO: Integrate with OpenAI or use heuristics for real decomposition
            var subtasks = new List<string>
            {
                $"Research steps for: {goalTitle}",
                $"Break down milestones for: {goalTitle}",
                $"Schedule review for: {goalTitle}"
            };
            return await Task.FromResult(new GoalDecompositionDto(goalId, goalTitle, subtasks, "Stubbed decomposition. Replace with AI/heuristics."));
        }
    }
}

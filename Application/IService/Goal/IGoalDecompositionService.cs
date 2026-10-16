using Application.DTOs.GoalDtos;

namespace Application.IService.Goal
{
    public interface IGoalDecompositionService
    {
        /// <summary>
        /// Decomposes a stored goal the caller owns. Returns null when the goal
        /// does not exist or belongs to another user.
        /// </summary>
        Task<GoalDecompositionDto?> DecomposeGoalAsync(Guid goalId, CancellationToken ct = default);
    }
}

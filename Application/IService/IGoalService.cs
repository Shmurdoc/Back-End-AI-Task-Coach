using Application.DTOs.GoalDtos;

namespace Application.IService;

public interface IGoalService
{
    Task<GoalDto?> GetGoalByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GoalDto>> GetUserGoalsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GoalDto> CreateGoalAsync(CreateGoalDto dto, CancellationToken cancellationToken = default);
    Task<GoalDto> UpdateGoalAsync(Guid id, UpdateGoalDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteGoalAsync(Guid id, CancellationToken cancellationToken = default);
}

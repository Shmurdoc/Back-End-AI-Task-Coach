using Domain.Entities;

namespace Application.IRepositories;

/// <summary>
/// Goal repository interface for goal management operations
/// </summary>
public interface IGoalRepository
{

    Task<Goal?> GetByIdAsync(Guid id);
    Task<IEnumerable<Goal>> GetUserGoalsAsync(Guid userId);
    Task<IEnumerable<Goal>> GetActiveUserGoalsAsync(Guid userId);
    Task<Goal> AddAsync(Goal goal);
    Task<Goal> UpdateAsync(Goal goal);
    Task<bool> DeleteAsync(Guid id);
    
    // Gamification methods
    Task<int> GetGoalCountByUserAsync(Guid userId);
    Task<int> GetCompletedGoalCountByUserAsync(Guid userId);
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

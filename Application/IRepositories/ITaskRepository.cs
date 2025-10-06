using Domain.Entities;

namespace Application.IRepositories;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetUserTasksAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetActiveUserTasksAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetGoalTasksAsync(Guid goalId);
    Task<TaskItem> AddAsync(TaskItem task);
    Task<TaskItem> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(Guid id);
    
    // Gamification methods
    Task<int> GetTaskCountByUserAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetRecentTasksByUserAsync(Guid userId, int days);
    Task<int> GetCompletedTaskCountByUserAsync(Guid userId);
}

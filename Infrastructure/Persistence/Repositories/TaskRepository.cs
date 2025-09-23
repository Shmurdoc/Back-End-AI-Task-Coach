

using Application.IRepositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Task repository implementation
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.Goal)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TaskItem>> GetUserTasksAsync(Guid userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .Include(t => t.Goal)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetActiveUserTasksAsync(Guid userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId && t.Status != TaskItemStatus.Completed && t.Status != TaskItemStatus.Cancelled)
            .Include(t => t.Goal)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CompletedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetGoalTasksAsync(Guid goalId)
    {
        return await _context.Tasks
            .Where(t => t.GoalId == goalId)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CompletedAt)
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        if (task.UserId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty");

        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        if (task.Status == TaskItemStatus.Completed && task.CompletedAt == null)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            task.Status = TaskItemStatus.Cancelled;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    // Gamification methods
    public async Task<int> GetTaskCountByUserAsync(Guid userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .CountAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetRecentTasksByUserAsync(Guid userId, int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Tasks
            .Where(t => t.UserId == userId && t.CompletedAt >= cutoffDate)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync();
    }

    public async Task<int> GetCompletedTaskCountByUserAsync(Guid userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId && t.CompletedAt.HasValue)
            .CountAsync();
    }
}

using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Goal repository implementation
/// </summary>
public class GoalRepository : IGoalRepository
{
    private readonly ApplicationDbContext _context;

    public GoalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Goal?> GetByIdAsync(Guid id)
    {
        return await _context.Goals
            .Include(g => g.Tasks)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Goal>> GetUserGoalsAsync(Guid userId)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId)
            .Include(g => g.Tasks)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Goal>> GetActiveUserGoalsAsync(Guid userId)
    {
        return await _context.Goals
            .Where(g => g.UserId == userId && g.Status == GoalStatus.InProgress)
            .Include(g => g.Tasks)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<Goal> AddAsync(Goal goal)
    {
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Goal> UpdateAsync(Goal goal)
    {
        goal.UpdatedAt = DateTime.UtcNow;
        if (goal.Status == GoalStatus.Completed && goal.CompletedAt == null)
        {
            goal.CompletedAt = DateTime.UtcNow;
        }
        _context.Goals.Update(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task DeleteAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal != null)
        {
            goal.Status = GoalStatus.Cancelled;
            goal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId);
    }
}

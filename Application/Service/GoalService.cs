using Application.CQRS.Features.Queries.Goals;
using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;



public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUserContext _userContext;

    public GoalService(IGoalRepository goalRepository, IUserContext userContext)
    {
        _goalRepository = goalRepository;
        _userContext = userContext;
    }

    public async Task<GoalDto> CreateGoalAsync(CreateGoalDto request, CancellationToken cancellationToken)
    {
        // ...existing validation logic if any...

        var userId = _userContext.GetCurrentUserId();
        var goal = new Domain.Entities.Goal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Category = request.Category,
            Priority = request.Priority,
            TargetDate = request.TargetDate,
            Status = Domain.Enums.GoalStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _goalRepository.AddAsync(goal);
        await _goalRepository.SaveChangesAsync(cancellationToken);


        // Calculate progress (0 for new goal)
        return new GoalDto(
            goal.Id, goal.Title, goal.Description, goal.Category, goal.Status,
            goal.Priority, goal.TargetDate, goal.CreatedAt, goal.CompletedAt, 0
        );
    }

    public async Task CompleteGoalAsync(Guid goalId)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null) throw new InvalidOperationException("Goal not found.");
        goal.MarkAsCompleted();
        await _goalRepository.UpdateAsync(goal);
    }

    public Task<IEnumerable<GoalDto>> GetUserGoal(Guid userId)
    {
        var goal = _goalRepository.GetUserGoalsAsync(userId).Result.FirstOrDefault();
        double progress = 0;
        if (goal == null)
        {
            return Task.FromResult<IEnumerable<GoalDto>>(new List<GoalDto>());
        }

        if (goal.Tasks != null && goal.Tasks.Count > 0)
        {
            var completed = goal.Tasks.Count(t => t.Status == TaskItemStatus.Completed);
            progress = Math.Round((double)completed / goal.Tasks.Count * 100, 2);

            goal.Progress = (int)progress; // Update the goal's progress based on tasks
        }
        return Task.FromResult<IEnumerable<GoalDto>>(
            new List<GoalDto>
            {
                new GoalDto(
                    goal.Id, goal.Title, goal.Description, goal.Category, goal.Status,
                    goal.Priority, goal.TargetDate, goal.CreatedAt, goal.CompletedAt, (int)progress
                )
            }
        );
    }

    public async Task<GoalDto?> GetGoalByIdAsync(Guid goalId, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null) return null;
        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.Category,
            goal.Status,
            goal.Priority,
            goal.TargetDate,
            goal.CreatedAt,
            goal.CompletedAt,
            goal.Progress
        );
    }

    public async Task<GoalDto> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(goalId);
        if (goal == null) throw new InvalidOperationException("Goal not found.");

        if (dto.Title is not null) goal.Title = dto.Title;
        if (dto.Description is not null) goal.Description = dto.Description;
        if (dto.Category is not null) goal.Category = dto.Category.Value;
        if (dto.Status is not null) goal.Status = dto.Status.Value;
        if (dto.Priority is not null) goal.Priority = dto.Priority.Value;
        if (dto.TargetDate is not null) goal.TargetDate = dto.TargetDate;
        if (dto.CompletedAt is not null) goal.CompletedAt = dto.CompletedAt;
        if (dto.Progress is not null) goal.Progress = (int)dto.Progress.Value;

        var updated = await _goalRepository.UpdateAsync(goal);
        return new GoalDto(
            updated.Id,
            updated.Title,
            updated.Description,
            updated.Category,
            updated.Status,
            updated.Priority,
            updated.TargetDate,
            updated.CreatedAt,
            updated.CompletedAt,
            updated.Progress
        );
    }

    public async Task<bool> DeleteGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        await _goalRepository.DeleteAsync(goalId);
        return true;
    }

    // ...other business logic methods as needed...
}

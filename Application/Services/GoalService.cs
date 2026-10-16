using Application.DTOs.GoalDtos;
using Application.IRepositories;
using Application.IService;
using Domain.Enums;
using FluentValidation;
// Application.IService.Goal (a namespace) shadows Domain.Entities.Goal here —
// without this alias, 'Goal' resolves to the namespace and fails to compile.
using GoalEntity = Domain.Entities.Goal;

namespace Application.Services;

public class GoalService : IGoalService
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateGoalDto> _createValidator;
    private readonly IUserContext _userContext;

    public GoalService(
        IGoalRepository goalRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateGoalDto> createValidator,
        IUserContext userContext)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _userContext = userContext;
    }

    public async Task<GoalDto?> GetGoalByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null || goal.UserId != _userContext.GetCurrentUserId()) return null;
        return MapToDto(goal);
    }

    public async Task<IEnumerable<GoalDto>> GetUserGoalsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId != _userContext.GetCurrentUserId()) return Enumerable.Empty<GoalDto>();
        var goals = await _goalRepository.GetUserGoalsAsync(userId);
        return goals.Select(MapToDto);
    }

    public async Task<GoalDto> CreateGoalAsync(CreateGoalDto dto, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var goal = new GoalEntity
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.GetCurrentUserId(),
            Title = dto.Title,
            Description = dto.Description ?? string.Empty,
            Category = dto.Category,
            Priority = dto.Priority,
            TargetDate = dto.TargetDate,
            Status = GoalStatus.NotStarted
        };

        var created = await _goalRepository.AddAsync(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(created);
    }

    public async Task<GoalDto> UpdateGoalAsync(Guid id, UpdateGoalDto dto, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Goal not found.");
        if (goal.UserId != _userContext.GetCurrentUserId())
            throw new InvalidOperationException("Goal not found.");

        if (dto.Title is not null) goal.Title = dto.Title;
        if (dto.Description is not null) goal.Description = dto.Description;
        if (dto.Category is not null) goal.Category = dto.Category.Value;
        if (dto.Status is not null) goal.Status = dto.Status.Value;
        if (dto.Priority is not null) goal.Priority = dto.Priority.Value;
        if (dto.TargetDate is not null) goal.TargetDate = dto.TargetDate;
        if (dto.CompletedAt is not null) goal.CompletedAt = dto.CompletedAt;
        if (dto.Progress is not null) goal.Progress = (int)dto.Progress.Value;

        var updated = await _goalRepository.UpdateAsync(goal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteGoalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _goalRepository.GetByIdAsync(id);
        if (goal == null || goal.UserId != _userContext.GetCurrentUserId())
            return false;

        var deleted = await _goalRepository.DeleteAsync(id);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return deleted;
    }

    private static GoalDto MapToDto(GoalEntity goal) => new(
        goal.Id,
        goal.Title,
        goal.Description,
        goal.Category,
        goal.Status,
        goal.Priority,
        goal.TargetDate,
        goal.CreatedAt,
        goal.CompletedAt,
        goal.Progress);
}

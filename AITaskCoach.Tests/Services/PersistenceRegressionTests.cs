using Application.DTOs.GoalDtos;
using Application.DTOs.HabitDtos;
using Application.DTOs.TaskDtos;
using Application.IRepositories;
using Application.IService;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AITaskCoach.Tests.Services;

// Every service write is asserted from a FRESH context over the same store.
// Unsaved entities are invisible to a query, so these tests fail on the old
// CQRS code (which mutated tracked entities without ever calling SaveChanges)
// and pass only when the UnitOfWork actually persists the change.
public class PersistenceRegressionTests : IDisposable
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly ApplicationDbContext _serviceContext;
    private readonly TaskService _taskService;
    private readonly GoalService _goalService;
    private readonly HabitService _habitService;

    public PersistenceRegressionTests()
    {
        _serviceContext = NewContext();
        var unitOfWork = new UnitOfWork(_serviceContext);

        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        _taskService = new TaskService(
            new TaskRepository(_serviceContext),
            unitOfWork,
            new CreateTaskDtoValidator(),
            new UpdateTaskDtoValidator(),
            userContext.Object);

        _goalService = new GoalService(
            new GoalRepository(_serviceContext),
            unitOfWork,
            new CreateGoalDtoValidator(),
            userContext.Object);

        _habitService = new HabitService(
            new HabitRepository(_serviceContext),
            unitOfWork,
            userContext.Object);
    }

    private ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options);

    private static CreateTaskDto NewTaskDto(string title = "Persisted task")
        => new(UserId, title, "desc", TaskItemStatus.Todo, TaskPriority.Medium,
            1.5, null, null, null, 3, 25, null);

    [Fact]
    public async Task CreateTask_PersistsRow_VisibleToFreshContext()
    {
        var created = await _taskService.CreateTaskAsync(NewTaskDto("Ship Cycle 1"));

        await using var verify = NewContext();
        var stored = await verify.Tasks.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Ship Cycle 1");
    }

    [Fact]
    public async Task UpdateTask_PersistsChanges_VisibleToFreshContext()
    {
        var created = await _taskService.CreateTaskAsync(NewTaskDto());

        var updated = await _taskService.UpdateTaskAsync(
            created.Id, new UpdateTaskDto(Title: "Renamed", CompletionPercentage: 55));

        await using var verify = NewContext();
        var stored = await verify.Tasks.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Renamed");
        stored.CompletionPercentage.Should().Be(55);
    }

    [Fact]
    public async Task DeleteTask_PersistsSoftDelete_VisibleToFreshContext()
    {
        var created = await _taskService.CreateTaskAsync(NewTaskDto());

        var deleted = await _taskService.DeleteTaskAsync(created.Id);

        deleted.Should().BeTrue();
        await using var verify = NewContext();
        var stored = await verify.Tasks.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(TaskItemStatus.Cancelled);
    }

    [Fact]
    public async Task CreateGoal_PersistsRow_VisibleToFreshContext()
    {
        var created = await _goalService.CreateGoalAsync(new CreateGoalDto(
            UserId, "Learn EF Core", null, GoalCategory.Professional, 2, null));

        await using var verify = NewContext();
        var stored = await verify.Goals.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Learn EF Core");
    }

    [Fact]
    public async Task UpdateGoal_PersistsChanges_VisibleToFreshContext()
    {
        var created = await _goalService.CreateGoalAsync(new CreateGoalDto(
            UserId, "Original", null, GoalCategory.Professional, 2, null));

        var updated = await _goalService.UpdateGoalAsync(
            created.Id, new UpdateGoalDto(Title: "Updated title", Progress: 40));

        await using var verify = NewContext();
        var stored = await verify.Goals.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Updated title");
        stored.Progress.Should().Be(40);
    }

    [Fact]
    public async Task DeleteGoal_PersistsSoftDelete_VisibleToFreshContext()
    {
        var created = await _goalService.CreateGoalAsync(new CreateGoalDto(
            UserId, "Doomed goal", null, GoalCategory.Professional, 2, null));

        var deleted = await _goalService.DeleteGoalAsync(created.Id);

        deleted.Should().BeTrue();
        await using var verify = NewContext();
        var stored = await verify.Goals.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(GoalStatus.Cancelled);
    }

    private static CreateHabitDto NewHabitDto(string name = "Drink water")
        => new(name, null, HabitFrequency.Daily, null, 8, "glasses", "#3498db",
            "droplet", null, HabitCategory.Health, null, null, 2, null);

    [Fact]
    public async Task CreateHabit_PersistsRow_VisibleToFreshContext()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        created.Should().NotBeNull();
        await using var verify = NewContext();
        var stored = await verify.Habits.FindAsync(created!.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Drink water");
    }

    [Fact]
    public async Task TrackHabitProgress_PersistsStreakChange_VisibleToFreshContext()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        await _habitService.TrackHabitProgressAsync(created!.Id, 8);

        await using var verify = NewContext();
        var stored = await verify.Habits.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.CurrentStreak.Should().Be(1);
    }

    [Fact]
    public async Task UpdateHabit_PersistsChanges_VisibleToFreshContext()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        await _habitService.UpdateHabitAsync(
            created!.Id, new UpdateHabitDto(Name: "Renamed habit"), CancellationToken.None);

        await using var verify = NewContext();
        var stored = await verify.Habits.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Renamed habit");
    }

    [Fact]
    public async Task DeleteHabit_PersistsDeactivation_VisibleToFreshContext()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        var deleted = await _habitService.DeleteHabitAsync(created!.Id, CancellationToken.None);

        deleted.Should().BeTrue();
        await using var verify = NewContext();
        var stored = await verify.Habits.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.IsActive.Should().BeFalse();
    }

    public void Dispose()
    {
        _serviceContext.Dispose();
    }
}

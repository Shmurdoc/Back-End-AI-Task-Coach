using Application.IRepositories;
using Application.Services;
using Application.Validators;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AITaskCoach.Tests.Services;

// Cross-user access must fail at the service layer even if a controller
// (or an internal caller) skips its own guard.
public class OwnershipTests : IDisposable
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid IntruderId = Guid.NewGuid();

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly List<ApplicationDbContext> _openContexts = new();

    private (TaskService Tasks, GoalService Goals, HabitService Habits) ServicesFor(Guid userId)
    {
        var ctx = NewContext();
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var uow = new UnitOfWork(ctx);

        return (
            new TaskService(new TaskRepository(ctx), uow,
                new CreateTaskDtoValidator(), new UpdateTaskDtoValidator(), userContext.Object),
            new GoalService(new GoalRepository(ctx), uow,
                new CreateGoalDtoValidator(), userContext.Object),
            new HabitService(new HabitRepository(ctx), uow, userContext.Object));
    }

    private ApplicationDbContext NewContext()
    {
        var ctx = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options);
        _openContexts.Add(ctx);
        return ctx;
    }

    private static Domain.Enums.TaskItemStatus Todo => Domain.Enums.TaskItemStatus.Todo;

    private static Application.DTOs.TaskDtos.CreateTaskDto TaskDtoFor(Guid userId, string title = "Owned")
        => new(userId, title, null, Todo, Domain.Enums.TaskPriority.Medium,
            1.0, null, null, null, 3, 25, null);

    private static Application.DTOs.GoalDtos.CreateGoalDto GoalDtoFor(Guid userId, string title = "Owned goal")
        => new(userId, title, null, Domain.Enums.GoalCategory.Professional, 2, null);

    private static Application.DTOs.HabitDtos.CreateHabitDto HabitDtoFor(string name = "Owned habit")
        => new(name, null, Domain.Enums.HabitFrequency.Daily, null, 8, "glasses", "#3498db",
            "droplet", null, Domain.Enums.HabitCategory.Health, null, null, 2, null);

    // ---------- tasks ----------

    [Fact]
    public async Task GetTask_WhenOwnedByAnotherUser_ReturnsNull()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Tasks.CreateTaskAsync(TaskDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var result = await intruder.Tasks.GetTaskByIdAsync(created.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTask_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Tasks.CreateTaskAsync(TaskDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var act = () => intruder.Tasks.UpdateTaskAsync(
            created.Id, new Application.DTOs.TaskDtos.UpdateTaskDto(Title: "Hijacked"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Task not found.");
    }

    [Fact]
    public async Task DeleteTask_WhenOwnedByAnotherUser_ReturnsFalse_AndRowSurvives()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Tasks.CreateTaskAsync(TaskDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var deleted = await intruder.Tasks.DeleteTaskAsync(created.Id);

        deleted.Should().BeFalse();
        await using var verify = NewContext();
        var stored = await verify.Tasks.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(Todo);
    }

    [Fact]
    public async Task CreateTask_DtoUserIdIsIgnored_CallerIdentityWins()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Tasks.CreateTaskAsync(TaskDtoFor(IntruderId, "Spoofed owner"));

        await using var verify = NewContext();
        var stored = await verify.Tasks.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task GetUserTasks_WhenAskingForAnotherUser_ReturnsEmpty()
    {
        var owner = ServicesFor(OwnerId);
        await owner.Tasks.CreateTaskAsync(TaskDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var result = (await intruder.Tasks.GetUserTasksAsync(OwnerId)).ToList();

        result.Should().BeEmpty();
    }

    // ---------- goals ----------

    [Fact]
    public async Task GetGoal_WhenOwnedByAnotherUser_ReturnsNull()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Goals.CreateGoalAsync(GoalDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var result = await intruder.Goals.GetGoalByIdAsync(created.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateGoal_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Goals.CreateGoalAsync(GoalDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var act = () => intruder.Goals.UpdateGoalAsync(
            created.Id, new Application.DTOs.GoalDtos.UpdateGoalDto(Title: "Hijacked"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Goal not found.");
    }

    [Fact]
    public async Task DeleteGoal_WhenOwnedByAnotherUser_ReturnsFalse_AndRowSurvives()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Goals.CreateGoalAsync(GoalDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var deleted = await intruder.Goals.DeleteGoalAsync(created.Id);

        deleted.Should().BeFalse();
        await using var verify = NewContext();
        var stored = await verify.Goals.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(Domain.Enums.GoalStatus.NotStarted);
    }

    [Fact]
    public async Task CreateGoal_DtoUserIdIsIgnored_CallerIdentityWins()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Goals.CreateGoalAsync(GoalDtoFor(IntruderId, "Spoofed owner"));

        await using var verify = NewContext();
        var stored = await verify.Goals.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task GetUserGoals_WhenAskingForAnotherUser_ReturnsEmpty()
    {
        var owner = ServicesFor(OwnerId);
        await owner.Goals.CreateGoalAsync(GoalDtoFor(OwnerId));

        var intruder = ServicesFor(IntruderId);
        var result = (await intruder.Goals.GetUserGoalsAsync(OwnerId)).ToList();

        result.Should().BeEmpty();
    }

    // ---------- habits ----------

    [Fact]
    public async Task GetHabit_WhenOwnedByAnotherUser_ReturnsNull()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Habits.CreateHabitAsync(HabitDtoFor(), CancellationToken.None);

        var intruder = ServicesFor(IntruderId);
        var result = await intruder.Habits.GetHabitByIdAsync(created!.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateHabit_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Habits.CreateHabitAsync(HabitDtoFor(), CancellationToken.None);

        var intruder = ServicesFor(IntruderId);
        var act = () => intruder.Habits.UpdateHabitAsync(
            created!.Id, new Application.DTOs.HabitDtos.UpdateHabitDto(Name: "Hijacked"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Habit not found.");
    }

    [Fact]
    public async Task DeleteHabit_WhenOwnedByAnotherUser_ReturnsFalse_AndRowSurvives()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Habits.CreateHabitAsync(HabitDtoFor(), CancellationToken.None);

        var intruder = ServicesFor(IntruderId);
        var deleted = await intruder.Habits.DeleteHabitAsync(created!.Id, CancellationToken.None);

        deleted.Should().BeFalse();
        await using var verify = NewContext();
        var stored = await verify.Habits.FindAsync(created.Id);
        stored.Should().NotBeNull();
        stored!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserHabits_WhenAskingForAnotherUser_ReturnsEmpty()
    {
        var owner = ServicesFor(OwnerId);
        await owner.Habits.CreateHabitAsync(HabitDtoFor(), CancellationToken.None);

        var intruder = ServicesFor(IntruderId);
        var result = (await intruder.Habits.GetUserHabitsAsync(OwnerId)).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TrackHabit_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        var owner = ServicesFor(OwnerId);
        var created = await owner.Habits.CreateHabitAsync(HabitDtoFor(), CancellationToken.None);

        var intruder = ServicesFor(IntruderId);
        var act = () => intruder.Habits.TrackHabitProgressAsync(created!.Id, 8);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Habit not found.");
    }

    public void Dispose()
    {
        foreach (var ctx in _openContexts)
            ctx.Dispose();
    }
}

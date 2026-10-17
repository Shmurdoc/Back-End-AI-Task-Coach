using Application.DTOs.HabitDtos;
using Application.IRepositories;
using Application.Services;
using Domain.Enums;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AITaskCoach.Tests.Services;

// The habit progress loop: TrackHabitProgressAsync must persist a real
// HabitEntry row and update streak / best-streak / last-completed fields, so
// the habit analytics endpoints read stored data instead of an empty store.
public class HabitProgressTests : IDisposable
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly string _dbName = Guid.NewGuid().ToString();
    private readonly ApplicationDbContext _serviceContext;
    private readonly HabitService _habitService;

    public HabitProgressTests()
    {
        _serviceContext = NewContext();
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        _habitService = new HabitService(
            new HabitRepository(_serviceContext),
            new UnitOfWork(_serviceContext),
            userContext.Object);
    }

    private ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options);

    private static CreateHabitDto NewHabitDto(int targetCount = 8)
        => new("Drink water", null, HabitFrequency.Daily, null, targetCount, "glasses", "#3498db",
            "droplet", null, HabitCategory.Health, null, null, 2, null);

    [Fact]
    public async Task TrackHabitProgress_MeetingTarget_WritesCompletedEntryAndUpdatesStreak()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        var (habit, completed) = await _habitService.TrackHabitProgressAsync(created!.Id, 8);

        completed.Should().BeTrue();
        habit.CurrentStreak.Should().Be(1);
        habit.BestStreak.Should().Be(1);
        habit.LastCompletedAt.Should().NotBeNull();

        await using var verify = NewContext();
        var storedHabit = await verify.Habits.FindAsync(created.Id);
        storedHabit!.CurrentStreak.Should().Be(1);
        storedHabit.BestStreak.Should().Be(1);
        storedHabit.LastCompletedAt.Should().NotBeNull();

        var entries = await verify.HabitEntries.Where(e => e.HabitId == created.Id).ToListAsync();
        entries.Should().ContainSingle();
        entries[0].Value.Should().Be(8);
        entries[0].Status.Should().Be(CompletionStatus.Completed);
    }

    [Fact]
    public async Task TrackHabitProgress_PartialValue_WritesPartialEntryAndKeepsStreakAtZero()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        var (habit, completed) = await _habitService.TrackHabitProgressAsync(created!.Id, 3);

        completed.Should().BeFalse();
        habit.CurrentStreak.Should().Be(0);

        await using var verify = NewContext();
        var entry = await verify.HabitEntries.SingleAsync(e => e.HabitId == created.Id);
        entry.Status.Should().Be(CompletionStatus.PartiallyCompleted);
        entry.Value.Should().Be(3);
    }

    [Fact]
    public async Task TrackHabitProgress_MissingTarget_ResetsStreakButKeepsBestStreak()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        await _habitService.TrackHabitProgressAsync(created!.Id, 8);
        await _habitService.TrackHabitProgressAsync(created.Id, 2);

        await using var verify = NewContext();
        var storedHabit = await verify.Habits.FindAsync(created.Id);
        storedHabit!.CurrentStreak.Should().Be(0);
        storedHabit.BestStreak.Should().Be(1);

        var statuses = await verify.HabitEntries
            .Where(e => e.HabitId == created.Id)
            .Select(e => e.Status)
            .ToListAsync();
        statuses.Should().BeEquivalentTo(new[] { CompletionStatus.Completed, CompletionStatus.PartiallyCompleted });
    }

    [Fact]
    public async Task TrackHabitProgress_NegativeValue_Throws()
    {
        var created = await _habitService.CreateHabitAsync(NewHabitDto(), CancellationToken.None);

        var act = () => _habitService.TrackHabitProgressAsync(created!.Id, -1);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    public void Dispose()
    {
        _serviceContext.Dispose();
    }
}

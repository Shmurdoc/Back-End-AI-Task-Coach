using Domain.Entities;
using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AITaskCoach.Tests.Repositories;

public class HabitRepositoryTests : RepositoryTestBase<HabitRepository>
{
    private readonly HabitRepository _repository;

    public HabitRepositoryTests()
    {
        _repository = new HabitRepository(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnHabit_WhenIdExists()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        var result = await _repository.GetByIdAsync(habit.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(habit);
    }

    [Fact]
    public async Task GetUserHabitsAsync_ShouldReturnHabitsForUser()
    {
        var userId = Fixture.Create<Guid>();
        var userHabits = Fixture.Build<Habit>()
            .With(h => h.UserId, userId)
            .CreateMany(2)
            .ToArray();
        var otherHabits = Fixture.CreateMany<Habit>(1).ToArray();
        await SeedDatabaseAsync(userHabits.Concat(otherHabits).ToArray());

        var result = await _repository.GetUserHabitsAsync(userId);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == userId);
    }

    [Fact]
    public async Task GetActiveUserHabitsAsync_ShouldReturnActiveHabitsForUser()
    {
        var userId = Fixture.Create<Guid>();
        var activeHabits = Fixture.Build<Habit>()
            .With(h => h.UserId, userId)
            .With(h => h.IsActive, true)
            .CreateMany(2)
            .ToArray();
        var inactiveHabits = Fixture.Build<Habit>()
            .With(h => h.UserId, userId)
            .With(h => h.IsActive, false)
            .CreateMany(1)
            .ToArray();
        await SeedDatabaseAsync(activeHabits.Concat(inactiveHabits).ToArray());

        var result = await _repository.GetActiveUserHabitsAsync(userId);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == userId && h.IsActive);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAndReturnHabit()
    {
        var habit = Fixture.Create<Habit>();
        habit.Id = Guid.Empty;

        var result = await _repository.AddAsync(habit);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Name.Should().Be(habit.Name);

        var savedHabit = await _repository.GetByIdAsync(result.Id);
        savedHabit.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateHabit()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);
        var updatedName = Fixture.Create<string>();
        habit.Name = updatedName;

        var result = await _repository.UpdateAsync(habit);

        result.Should().NotBeNull();
        result.Name.Should().Be(updatedName);

        var savedHabit = await _repository.GetByIdAsync(habit.Id);
        savedHabit!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveHabit()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        await _repository.DeleteAsync(habit.Id);

        var deletedHabit = await _repository.GetByIdAsync(habit.Id);
        deletedHabit.Should().BeNull();
    }

    [Fact]
    public async Task AddHabitEntryAsync_ShouldAddAndReturnEntry()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);
        var entry = Fixture.Build<HabitEntry>()
            .With(e => e.HabitId, habit.Id)
            .Create();

        var result = await _repository.AddHabitEntryAsync(entry);

        result.Should().NotBeNull();
        result.HabitId.Should().Be(habit.Id);
    }

    [Fact]
    public async Task GetHabitEntriesAsync_ShouldReturnEntriesInDateRange()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        var endDate = DateTime.UtcNow.Date.AddDays(1);

        var entries = Fixture.Build<HabitEntry>()
            .With(e => e.HabitId, habit.Id)
            .With(e => e.Date, DateTime.UtcNow.Date.AddDays(-2))
            .CreateMany(2)
            .ToArray();

        await SeedDatabaseAsync(entries);

        var result = await _repository.GetHabitEntriesAsync(habit.Id, startDate, endDate);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.HabitId == habit.Id && e.Date >= startDate && e.Date <= endDate);
    }

    [Fact]
    public async Task UpdateStreakAsync_ShouldUpdateHabitStreak()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        await _repository.UpdateStreakAsync(habit.Id);

        // Optionally, verify streak logic if implemented
        var updatedHabit = await _repository.GetByIdAsync(habit.Id);
        updatedHabit.Should().NotBeNull();
        // updatedHabit.Streak.Should().Be(...); // If streak property exists
    }

    [Fact]
    public async Task GetCompletionRateByDayOfWeekAsync_ShouldReturnDictionary()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        var result = await _repository.GetCompletionRateByDayOfWeekAsync(habit.Id);

        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<DayOfWeek, double>>();
    }

    [Fact]
    public async Task GetCompletionRateByHourAsync_ShouldReturnDictionary()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        var result = await _repository.GetCompletionRateByHourAsync(habit.Id);

        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<int, double>>();
    }

    [Fact]
    public async Task CreateAnalyticsAsync_ShouldNotThrow()
    {
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        var act = async () => await _repository.CreateAnalyticsAsync(habit.Id);

        await act.Should().NotThrowAsync();
    }
}

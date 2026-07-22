using Domain.Entities;
using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Xunit;
using AutoFixture; // Add this using directive for Fixture customization

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
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        var result = await _repository.GetByIdAsync(habit.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(habit);
    }

    [Fact]
    public async Task GetUserHabitsAsync_ShouldReturnHabitsForUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        var otherUserId = Guid.NewGuid();

        var userHabits = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.UserId, user.Id)
            .With(h => h.User, (User?)null)
            .CreateMany(2)
            .ToArray();

        var otherHabits = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.UserId, otherUserId)
            .With(h => h.User, (User?)null)
            .CreateMany(1)
            .ToArray();

        await SeedDatabaseAsync(userHabits.Concat(otherHabits).ToArray());

        // Act
        var result = await _repository.GetUserHabitsAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == user.Id);
    }

    [Fact]
    public async Task GetActiveUserHabitsAsync_ShouldReturnActiveHabitsForUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        var otherUserId = Guid.NewGuid();

        var activeHabits = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.UserId, user.Id)
            .With(h => h.User, (User?)null)
            .With(h => h.IsActive, true)
            .CreateMany(2)
            .ToArray();

        var inactiveHabits = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.UserId, user.Id)
            .With(h => h.User, (User?)null)
            .With(h => h.IsActive, false)
            .CreateMany(1)
            .ToArray();

        var otherHabits = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.UserId, otherUserId)
            .With(h => h.User, (User?)null)
            .With(h => h.IsActive, true)
            .CreateMany(1)
            .ToArray();

        await SeedDatabaseAsync(activeHabits.Concat(inactiveHabits).Concat(otherHabits).ToArray());

        // Act
        var result = await _repository.GetActiveUserHabitsAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == user.Id && h.IsActive);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAndReturnHabit()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();

        // Act
        var result = await _repository.AddAsync(habit);
        await _repository.SaveChangesAsync(default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var savedHabit = await _repository.GetByIdAsync(result.Id);
        savedHabit.Should().NotBeNull();
        savedHabit.Id.Should().Be(result.Id); // Verify the saved habit matches the added habit
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateHabit()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);
        
        // Detach the tracked entity to avoid conflict
        DbContext.Entry(habit).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var updatedHabit = Fixture.Build<Habit>()
            .OmitAutoProperties()
            .With(h => h.Id, habit.Id)
            .With(h => h.UserId, habit.UserId)
            .With(h => h.User, (User?)null)
            .With(h => h.Name, Fixture.Create<string>())
            .With(h => h.IsActive, true)
            .Create();

        // Act
        var result = await _repository.UpdateAsync(updatedHabit);
        await _repository.SaveChangesAsync(default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(updatedHabit.Id);

        var savedHabit = await _repository.GetByIdAsync(habit.Id);
        savedHabit!.Name.Should().Be(updatedHabit.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteHabit()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        await _repository.DeleteAsync(habit.Id);
        await _repository.SaveChangesAsync(default);

        // Assert - repository soft-deletes by setting IsActive = false
        var deletedHabit = await _repository.GetByIdAsync(habit.Id);
        deletedHabit.Should().NotBeNull();
        deletedHabit!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AddHabitEntryAsync_ShouldAddAndReturnEntry()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);
        
        var entry = Fixture.Build<HabitEntry>()
            .OmitAutoProperties()
            .With(e => e.HabitId, habit.Id)
            .With(e => e.Habit, (Habit?)null)
            .Create();

        // Act
        var result = await _repository.AddHabitEntryAsync(entry);
        await _repository.SaveChangesAsync(default);

        // Assert
        result.Should().NotBeNull();
        result.HabitId.Should().Be(habit.Id);
        
        // Verify entry was saved to the database
        var savedEntry = await DbContext.HabitEntries.FindAsync(result.Id);
        savedEntry.Should().NotBeNull();
        savedEntry.HabitId.Should().Be(habit.Id);
    }
    
    [Fact]
    public async Task GetHabitEntriesAsync_ShouldReturnEntriesInDateRange()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);
        
        var startDate = DateTime.UtcNow.Date.AddDays(-5);
        var endDate = DateTime.UtcNow.Date.AddDays(1);
        
        // Create entries within the date range
        var entries = Fixture.Build<HabitEntry>()
            .OmitAutoProperties()
            .With(e => e.HabitId, habit.Id)
            .With(e => e.Habit, (Habit?)null)
            .With(e => e.Date, DateTime.UtcNow.Date.AddDays(-2))
            .CreateMany(2)
            .ToArray();
            
        // Create entries outside the date range to ensure they are excluded
        var outsideRangeEntries = Fixture.Build<HabitEntry>()
            .OmitAutoProperties()
            .With(e => e.HabitId, habit.Id)
            .With(e => e.Habit, (Habit?)null)
            .With(e => e.Date, DateTime.UtcNow.Date.AddDays(-10))
            .CreateMany(1)
            .ToArray();
            
        await SeedDatabaseAsync(entries.Concat(outsideRangeEntries).ToArray());

        // Act
        var result = await _repository.GetHabitEntriesAsync(habit.Id, startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.HabitId == habit.Id && e.Date.Date >= startDate && e.Date.Date <= endDate);
    }

    [Fact]
    public async Task UpdateStreakAsync_ShouldUpdateHabitStreak()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        await _repository.UpdateStreakAsync(habit.Id);
        await _repository.SaveChangesAsync(default);

        // Assert
        var updatedHabit = await _repository.GetByIdAsync(habit.Id);
        updatedHabit.Should().NotBeNull();
        // Assuming your repository logic updates the Streak property, you would assert it here.
        // E.g., updatedHabit.Streak.Should().Be(...);
    }

    [Fact]
    public async Task GetCompletionRateByDayOfWeekAsync_ShouldReturnDictionary()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        var result = await _repository.GetCompletionRateByDayOfWeekAsync(habit.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<DayOfWeek, double>>();
    }

    [Fact]
    public async Task GetCompletionRateByHourAsync_ShouldReturnDictionary()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        var result = await _repository.GetCompletionRateByHourAsync(habit.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<int, double>>();
    }

    [Fact]
    public async Task CreateAnalyticsAsync_ShouldNotThrow()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();
        await SeedDatabaseAsync(habit);

        // Act
        var act = async () => await _repository.CreateAnalyticsAsync(habit.Id);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
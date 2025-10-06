using Domain.Entities;
using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AutoFixture;

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
        // Arrange - create habits manually to avoid AutoFixture issues
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var habit1 = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Habit 1",
            Description = "First habit",
            Color = "#3498db",
            Icon = "icon1",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        var habit2 = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Habit 2",
            Description = "Second habit",
            Color = "#3498db",
            Icon = "icon2",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        var otherUserHabit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Name = "Other Habit",
            Description = "Other user's habit",
            Color = "#3498db",
            Icon = "icon3",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        await _repository.AddAsync(habit1);
        await _repository.AddAsync(habit2);
        await _repository.AddAsync(otherUserHabit);

        // Act
        var result = await _repository.GetUserHabitsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == userId);
        result.Should().Contain(h => h.Name == "Habit 1");
        result.Should().Contain(h => h.Name == "Habit 2");
    }

    [Fact]
    public async Task GetActiveUserHabitsAsync_ShouldReturnActiveHabitsForUser()
    {
        // Arrange - create habits manually to avoid AutoFixture issues
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var activeHabit1 = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Active Habit 1",
            Description = "First active habit",
            Color = "#3498db",
            Icon = "icon1",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        var activeHabit2 = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Active Habit 2",
            Description = "Second active habit",
            Color = "#3498db",
            Icon = "icon2",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        var inactiveHabit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Inactive Habit",
            Description = "Inactive habit",
            Color = "#3498db",
            Icon = "icon3",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = false,
            Category = Domain.Enums.HabitCategory.Personal
        };

        var otherUserHabit = new Habit
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Name = "Other User Habit",
            Description = "Other user's habit",
            Color = "#3498db",
            Icon = "icon4",
            Unit = "times",
            Frequency = Domain.Enums.HabitFrequency.Daily,
            TargetCount = 1,
            IsActive = true,
            Category = Domain.Enums.HabitCategory.Personal
        };

        await _repository.AddAsync(activeHabit1);
        await _repository.AddAsync(activeHabit2);
        await _repository.AddAsync(inactiveHabit);
        await _repository.AddAsync(otherUserHabit);

        // Act
        var result = await _repository.GetActiveUserHabitsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(h => h.UserId == userId && h.IsActive);
        result.Should().Contain(h => h.Name == "Active Habit 1");
        result.Should().Contain(h => h.Name == "Active Habit 2");
    }

    [Fact]
    public async Task AddAsync_ShouldAddAndReturnHabit()
    {
        // Arrange
        var habit = Fixture.Create<Habit>();

        // Act
        var result = await _repository.AddAsync(habit);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var savedHabit = await _repository.GetByIdAsync(result.Id);
        savedHabit.Should().NotBeNull();
        savedHabit!.Id.Should().Be(result.Id); // Verify the saved habit matches the added habit
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateHabit()
    {
        // Arrange
        var habit = Fixture.Build<Habit>()
            .Without(h => h.Analytics)
            .Without(h => h.Entries)
            .Create();
        await SeedDatabaseAsync(habit);
        
        // Get fresh instance to avoid tracking issues
        DbContext.ChangeTracker.Clear();
        var habitToUpdate = await _repository.GetByIdAsync(habit.Id);
        var updatedName = Fixture.Create<string>();
        habitToUpdate!.Name = updatedName;

        // Act
        var result = await _repository.UpdateAsync(habitToUpdate);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updatedName);

        // Clear tracking and verify
        DbContext.ChangeTracker.Clear();
        var savedHabit = await _repository.GetByIdAsync(habit.Id);
        savedHabit!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteHabit()
    {
        // Arrange
        var habit = Fixture.Build<Habit>()
            .Without(h => h.Analytics)
            .Without(h => h.Entries)
            .With(h => h.IsActive, true)
            .Create();
        await SeedDatabaseAsync(habit);

        // Act
        var result = await _repository.DeleteAsync(habit.Id);

        // Assert
        result.Should().BeTrue();
        DbContext.ChangeTracker.Clear();
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
            .With(e => e.HabitId, habit.Id)
            .Without(e => e.Habit)
            .Without(e => e.Id) // Let EF generate the ID
            .Create();

        // Act
        var result = await _repository.AddHabitEntryAsync(entry);

        // Assert
        result.Should().NotBeNull();
        result.HabitId.Should().Be(habit.Id);
        result.Id.Should().NotBe(Guid.Empty);
        
        // Verify entry was saved to the database
        var savedEntry = await DbContext.HabitEntries.FindAsync(result.Id);
        savedEntry.Should().NotBeNull();
        savedEntry!.HabitId.Should().Be(habit.Id);
    }
    
    [Fact]
    public async Task GetHabitEntriesAsync_ShouldReturnEntriesInDateRange()
    {
        // Arrange
        var habit = Fixture.Build<Habit>()
            .Without(h => h.Analytics)
            .Without(h => h.Entries)
            .Create();
        await SeedDatabaseAsync(habit);
        
        var startDate = DateTime.Today.AddDays(-5);
        var endDate = DateTime.Today.AddDays(1);
        
        // Create entries within the date range
        var entries = new[]
        {
            Fixture.Build<HabitEntry>()
                .With(e => e.HabitId, habit.Id)
                .With(e => e.Date, DateTime.Today.AddDays(-2))
                .Without(e => e.Habit)
                .Create(),
            Fixture.Build<HabitEntry>()
                .With(e => e.HabitId, habit.Id)
                .With(e => e.Date, DateTime.Today.AddDays(-1))
                .Without(e => e.Habit)
                .Create()
        };
            
        // Create entries outside the date range to ensure they are excluded
        var outsideRangeEntries = Fixture.Build<HabitEntry>()
            .With(e => e.HabitId, habit.Id)
            .With(e => e.Date, DateTime.Today.AddDays(-10))
            .Without(e => e.Habit)
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
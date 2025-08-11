
using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace AITaskCoach.Tests.Repositories;

public class GoalRepositoryTests : RepositoryTestBase<GoalRepository>
{
    private readonly GoalRepository _repository;

    public GoalRepositoryTests()
    {
        _repository = new GoalRepository(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGoal_WhenIdExists()
    {
        // Arrange
        var goal = Fixture.Create<Goal>();
        await SeedDatabaseAsync(goal);

        // Act
        var result = await _repository.GetByIdAsync(goal.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(goal);
    }

    [Fact]
    public async Task GetUserGoalsAsync_ShouldReturnGoalsForUser()
    {
        // Arrange
        var userId = Fixture.Create<Guid>();
        var userGoals = Fixture.Build<Goal>()
            .With(g => g.UserId, userId)
            .CreateMany(2)
            .ToArray();
        var otherGoals = Fixture.CreateMany<Goal>(1).ToArray();
        await SeedDatabaseAsync(userGoals.Concat(otherGoals).ToArray());

        // Act
        var result = await _repository.GetUserGoalsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => g.UserId == userId);
    }

    [Fact]
    public async Task GetActiveUserGoalsAsync_ShouldReturnActiveGoalsForUser()
    {
        // Arrange
        var userId = Fixture.Create<Guid>();
        var activeGoals = Fixture.Build<Goal>()
            .With(g => g.UserId, userId)
            .With(g => g.CreatedAt, DateTime.UtcNow)
            .CreateMany(2)
            .ToArray();
        var inactiveGoals = Fixture.Build<Goal>()
            .With(g => g.UserId, userId)
            .With(g => g.CreatedAt, DateTime.UtcNow)
            .CreateMany(1)
            .ToArray();
        await SeedDatabaseAsync(activeGoals.Concat(inactiveGoals).ToArray());

        // Act
        var result = await _repository.GetActiveUserGoalsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => g.UserId == userId && g.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAndReturnGoal()
    {
        // Arrange
        var goal = Fixture.Create<Goal>();
        goal.Id = Guid.Empty; // Reset Id for new entity

        // Act
        var result = await _repository.AddAsync(goal);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be(goal.Title);

        var savedGoal = await _repository.GetByIdAsync(result.Id);
        savedGoal.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGoal()
    {
        // Arrange
        var goal = Fixture.Create<Goal>();
        await SeedDatabaseAsync(goal);
        var updatedName = Fixture.Create<string>();
        goal.Title = updatedName;

        // Act
        var result = await _repository.UpdateAsync(goal);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(updatedName);

        var savedGoal = await _repository.GetByIdAsync(goal.Id);
        savedGoal!.Title.Should().Be(updatedName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveGoal()
    {
        // Arrange
        var goal = Fixture.Create<Goal>();
        await SeedDatabaseAsync(goal);

        // Act
        await _repository.DeleteAsync(goal.Id);

        // Assert
        var deletedGoal = await _repository.GetByIdAsync(goal.Id);
        deletedGoal.Should().BeNull();
    }
}

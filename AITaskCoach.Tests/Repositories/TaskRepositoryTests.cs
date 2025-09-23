using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AITaskCoach.Tests.Repositories;

public class TaskRepositoryTests : RepositoryTestBase<TaskRepository>
{
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        _repository = new TaskRepository(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var task = Fixture.Create<TaskItem>();
        await SeedDatabaseAsync(task);

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Fixture.Create<Guid>();

        // Act
        var result = await _repository.GetByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidTask_ShouldAddAndReturnTask()
    {
        // Arrange
        var task = Fixture.Create<TaskItem>();
        task.Id = Guid.Empty; // Reset Id for new entity

        // Act
        var result = await _repository.AddAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be(task.Title);

        var savedTask = await _repository.GetByIdAsync(result.Id);
        savedTask.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidTask_ShouldUpdateTask()
    {
        // Arrange
        var task = Fixture.Create<TaskItem>();
        await SeedDatabaseAsync(task);
        
        var updatedTitle = Fixture.Create<string>();
        task.Title = updatedTitle;

        // Act
        var result = await _repository.UpdateAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(updatedTitle);
        
        var savedTask = await _repository.GetByIdAsync(task.Id);
        savedTask!.Title.Should().Be(updatedTitle);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteTask()
    {
        // Arrange
        var task = Fixture.Create<TaskItem>();
        await SeedDatabaseAsync(task);

        // Act

    var result = await _repository.DeleteAsync(task.Id);

    // Assert
    result.Should().BeTrue();

        var deletedTask = await _repository.GetByIdAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var invalidId = Fixture.Create<Guid>();

        // Act
    var result = await _repository.DeleteAsync(invalidId);

    // Assert
    result.Should().BeFalse();
    }
}


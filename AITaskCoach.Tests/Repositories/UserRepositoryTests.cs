using AITaskCoach.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AITaskCoach.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase<UserRepository>
{
    private readonly UserRepository _repository;
    private readonly Mock<ILogger<UserRepository>> _loggerMock;

    public UserRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(DbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = Fixture.CreateMany<User>(3).ToArray();
        await SeedDatabaseAsync(users);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var invalidEmail = Fixture.Create<string>();

        // Act
        var result = await _repository.GetByEmailAsync(invalidEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldAddAndReturnUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        user.Id = Guid.Empty; // Reset Id for new entity

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Email.Should().Be(user.Email);
        
        var savedUser = await _repository.GetByIdAsync(result.Id);
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_ShouldUpdateUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);
        
        var updatedName = Fixture.Create<string>();
        user.Name = updatedName;

        // Act
        var result = await _repository.UpdateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updatedName);
        
        var savedUser = await _repository.GetByIdAsync(user.Id);
        savedUser!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        // Act
        await _repository.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await _repository.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var user = Fixture.Create<User>();
        await SeedDatabaseAsync(user);

        // Act
        var result = await _repository.ExistsAsync(user.Email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Arrange
        var email = Fixture.Create<string>();

        // Act
        var result = await _repository.ExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveUsersAsync_ShouldReturnActiveUsers()
    {
        // Arrange
        var activeUsers = Fixture.Build<User>()
            .With(u => u.IsActive, true)
            .CreateMany(2)
            .ToArray();
        var inactiveUsers = Fixture.Build<User>()
            .With(u => u.IsActive, false)
            .CreateMany(1)
            .ToArray();
        await SeedDatabaseAsync(activeUsers.Concat(inactiveUsers).ToArray());

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
    }
}

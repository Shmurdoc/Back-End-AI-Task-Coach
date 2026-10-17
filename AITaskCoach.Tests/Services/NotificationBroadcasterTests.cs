using Application.DTOs.GoalDtos;
using Application.DTOs.HabitDtos;
using Application.DTOs.TaskDtos;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using WebAPI.Hubs;
using WebAPI.Services;

namespace AITaskCoach.Tests.Services;

public class NotificationBroadcasterTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static (NotificationBroadcaster Broadcaster, Mock<IClientProxy> Client, Mock<IHubClients> Clients)
        CreateBroadcaster()
    {
        var client = new Mock<IClientProxy>();
        client.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clients = new Mock<IHubClients>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(client.Object);

        var hubContext = new Mock<IHubContext<NotificationHub>>();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);

        var logger = new Mock<ILogger<NotificationBroadcaster>>();
        return (new NotificationBroadcaster(hubContext.Object, logger.Object), client, clients);
    }

    private static TaskDto NewTaskDto()
        => new(Guid.NewGuid(), "Ship it", "desc", TaskItemStatus.Completed, TaskPriority.Medium,
            1.5, 1.5, 100, DateTime.UtcNow, null, Array.Empty<string>(), null, Array.Empty<Guid>(),
            3, 25, UserId, null, DateTime.UtcNow, DateTime.UtcNow);

    private static GoalDto NewGoalDto()
        => new(Guid.NewGuid(), "Run a marathon", null, GoalCategory.Health, GoalStatus.Completed,
            1, null, DateTime.UtcNow, DateTime.UtcNow, 100);

    private static HabitDto NewHabitDto()
        => new(Guid.NewGuid(), "Drink water", "desc", HabitFrequency.Daily, null, 8, "glasses",
            "#3498db", "droplet", "Stay hydrated", 1, 1, 100, true, HabitCategory.Health,
            Array.Empty<string>(), Array.Empty<string>(), null, 2, null, DateTime.UtcNow,
            UserId, DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task TaskCompleted_SendsCompactPayloadToOwnersGroup()
    {
        var (broadcaster, client, clients) = CreateBroadcaster();
        var task = NewTaskDto();

        object?[]? capturedArgs = null;
        client.Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((_, args, _) => capturedArgs = args)
            .Returns(Task.CompletedTask);

        await broadcaster.TaskCompletedAsync(UserId, task);

        client.Verify(c => c.SendCoreAsync("TaskCompleted", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
        clients.Verify(c => c.Group($"user:{UserId}"), Times.Once);
        capturedArgs.Should().NotBeNull();
        var args = capturedArgs!;
        args.Should().ContainSingle();
        var payload = args[0]!;
        payload.GetType().GetProperty("Id")!.GetValue(payload).Should().Be(task.Id);
        payload.GetType().GetProperty("Title")!.GetValue(payload).Should().Be("Ship it");
    }

    [Fact]
    public async Task GoalCompleted_SendsToOwnersGroup()
    {
        var (broadcaster, client, clients) = CreateBroadcaster();

        await broadcaster.GoalCompletedAsync(UserId, NewGoalDto());

        client.Verify(c => c.SendCoreAsync("GoalCompleted", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
        clients.Verify(c => c.Group($"user:{UserId}"), Times.Once);
    }

    [Fact]
    public async Task HabitCompleted_SendsToOwnersGroup()
    {
        var (broadcaster, client, clients) = CreateBroadcaster();

        await broadcaster.HabitCompletedAsync(UserId, NewHabitDto());

        client.Verify(c => c.SendCoreAsync("HabitCompleted", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
        clients.Verify(c => c.Group($"user:{UserId}"), Times.Once);
    }
}

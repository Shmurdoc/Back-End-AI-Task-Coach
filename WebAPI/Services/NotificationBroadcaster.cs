using Application.DTOs.GoalDtos;
using Application.DTOs.HabitDtos;
using Application.DTOs.TaskDtos;
using Microsoft.AspNetCore.SignalR;
using Prometheus;
using WebAPI.Hubs;

namespace WebAPI.Services;

/// <summary>
/// Sends real-time notifications to a user's SignalR group. Events are named
/// after domain completions and carry a compact payload so clients can react
/// without re-fetching the full resource.
/// </summary>
public interface INotificationBroadcaster
{
    Task TaskCompletedAsync(Guid userId, TaskDto task, CancellationToken cancellationToken = default);
    Task GoalCompletedAsync(Guid userId, GoalDto goal, CancellationToken cancellationToken = default);
    Task HabitCompletedAsync(Guid userId, HabitDto habit, CancellationToken cancellationToken = default);
}

public sealed class NotificationBroadcaster : INotificationBroadcaster
{
    private static readonly Counter NotificationsSent = Metrics.CreateCounter(
        "notifications_sent_total",
        "Real-time notifications delivered over SignalR, labelled by event type.",
        new CounterConfiguration { LabelNames = new[] { "type" } });

    private static readonly Histogram DeliveryTime = Metrics.CreateHistogram(
        "notification_delivery_time_seconds",
        "Time spent enqueuing a SignalR notification.",
        new HistogramConfiguration { LabelNames = new[] { "type" } });

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationBroadcaster> _logger;

    public NotificationBroadcaster(IHubContext<NotificationHub> hubContext, ILogger<NotificationBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task TaskCompletedAsync(Guid userId, TaskDto task, CancellationToken cancellationToken = default)
        => SendAsync(userId, "task_completed", "TaskCompleted",
            new { task.Id, task.Title, task.Status, task.CompletedAt }, cancellationToken);

    public Task GoalCompletedAsync(Guid userId, GoalDto goal, CancellationToken cancellationToken = default)
        => SendAsync(userId, "goal_completed", "GoalCompleted",
            new { goal.Id, goal.Title, goal.Status, goal.CompletedAt }, cancellationToken);

    public Task HabitCompletedAsync(Guid userId, HabitDto habit, CancellationToken cancellationToken = default)
        => SendAsync(userId, "habit_completed", "HabitCompleted",
            new { habit.Id, habit.Name, habit.CurrentStreak, habit.CompletionRate, habit.LastCompletedAt }, cancellationToken);

    private async Task SendAsync(Guid userId, string metricType, string eventName, object payload, CancellationToken cancellationToken)
    {
        using var timer = DeliveryTime.WithLabels(metricType).NewTimer();

        await _hubContext.Clients
            .Group(NotificationHub.UserGroup(userId.ToString()))
            .SendAsync(eventName, payload, cancellationToken);

        NotificationsSent.WithLabels(metricType).Inc();
        _logger.LogInformation("SignalR event {EventName} sent to user {UserId}", eventName, userId);
    }
}

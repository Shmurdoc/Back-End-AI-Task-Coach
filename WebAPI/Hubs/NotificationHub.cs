using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Group name for a user's notification channel. Shared with the
    /// notification broadcaster so hub membership and server-side delivery
    /// always target the same group.
    /// </summary>
    public static string UserGroup(string userId) => $"user:{userId}";

    /// <summary>
    /// Subscribes the calling connection to its own notification group.
    /// The user identity comes from the authenticated connection, never from
    /// the client, so a connection can only ever join its own group.
    /// </summary>
    public async Task JoinUserGroup()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthenticated connection {ConnectionId} attempted to join a notification group", Context.ConnectionId);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        _logger.LogInformation("User {UserId} joined notification group with connection {ConnectionId}",
            userId, Context.ConnectionId);
    }

    /// <summary>
    /// Removes the calling connection from its notification group.
    /// </summary>
    public async Task LeaveUserGroup()
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));
        _logger.LogInformation("User {UserId} left notification group with connection {ConnectionId}",
            userId, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

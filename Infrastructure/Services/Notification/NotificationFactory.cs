using Application.IService;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Notification;

public class NotificationFactory
{
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly ILogger<NotificationFactory> _logger;

    public NotificationFactory(IEnumerable<INotificationProvider> providers, ILogger<NotificationFactory> logger)
    { _providers = providers; _logger = logger; }

    public INotificationProvider? Get(string name)
    {
        var p = _providers.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (p == null) _logger.LogWarning("Notification provider {Name} not found", name);
        return p;
    }
}

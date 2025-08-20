namespace Application.Services;

public interface IWebhookService
{
    Task TriggerWebhookAsync(string url, object payload);
}

public class WebhookService : IWebhookService
{
    public async Task TriggerWebhookAsync(string url, object payload)
    {
        // TODO: Implement webhook POST logic
        await Task.CompletedTask;
    }
}

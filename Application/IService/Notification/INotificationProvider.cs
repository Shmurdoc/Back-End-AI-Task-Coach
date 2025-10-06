using Domain.Entities;
namespace Application.IService;
public interface INotificationProvider
{
    Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default);
    string Name { get; }
}

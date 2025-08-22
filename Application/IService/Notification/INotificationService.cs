using Domain.Entities;
namespace Application.IService;
public interface INotificationService
{
    Task<bool> SendAsync(User user, string subject, string message, CancellationToken ct = default);
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.IService.Focus
{
    /// <summary>
    /// Interface for integrating with external focus tools (e.g., Pomodoro apps).
    /// </summary>
    public interface IFocusToolIntegrationService
    {
        Task<bool> StartFocusSessionAsync(Guid userId, int minutes, CancellationToken ct = default);
        Task<bool> EndFocusSessionAsync(Guid userId, CancellationToken ct = default);
        Task<bool> SyncFocusSessionAsync(Guid userId, DateTime start, DateTime end, CancellationToken ct = default);
    }
}

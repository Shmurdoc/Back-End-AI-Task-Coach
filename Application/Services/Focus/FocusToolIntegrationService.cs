using Application.IService.Focus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.Focus
{
    /// <summary>
    /// Stub implementation for integrating with external focus tools (e.g., Pomodoro apps).
    /// </summary>
    public class FocusToolIntegrationService : IFocusToolIntegrationService
    {
        public async Task<bool> StartFocusSessionAsync(Guid userId, int minutes, CancellationToken ct = default)
        {
            // TODO: Integrate with external focus tool API
            await Task.Delay(100, ct); // Simulate async
            return true;
        }

        public async Task<bool> EndFocusSessionAsync(Guid userId, CancellationToken ct = default)
        {
            // TODO: Integrate with external focus tool API
            await Task.Delay(100, ct);
            return true;
        }

        public async Task<bool> SyncFocusSessionAsync(Guid userId, DateTime start, DateTime end, CancellationToken ct = default)
        {
            // TODO: Integrate with external focus tool API
            await Task.Delay(100, ct);
            return true;
        }
    }
}

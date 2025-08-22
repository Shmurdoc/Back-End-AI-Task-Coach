using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Service;

public interface INudgeOrchestrator
{
    Task OrchestrateNudgeAsync(Guid userId, CancellationToken cancellationToken = default);
}

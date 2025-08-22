using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.IService;

public interface IAdaptiveSchedulingEngine
{
    Task RescheduleAsync(Guid userId, CancellationToken cancellationToken = default);
}

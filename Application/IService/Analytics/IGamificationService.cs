using Application.DTOs.Analytics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.IService.Analytics
{
    public interface IGamificationService
    {
        Task<GamificationSummaryDto> GetGamificationSummaryAsync(Guid userId, CancellationToken ct = default);
    }
}

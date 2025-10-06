using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IAnalyticsService
    {
        Task<ProductivitySummaryDto> CalculateUserStats(Guid userId);

        // Controller-compatible stubs
        Task<ProductivitySummaryDto> GetProductivitySummaryAsync(Guid userId, CancellationToken cancellationToken = default);
        Task GenerateUserReportAsync(Guid userId, CancellationToken cancellationToken = default);
        Task GetHabitSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

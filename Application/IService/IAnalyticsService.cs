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
        Task<ProductivitySummaryDto> GetProductivitySummaryAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserReportDto> GenerateUserReportAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<HabitSummaryDto> GetHabitSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

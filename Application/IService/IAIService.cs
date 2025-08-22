using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IAIService
    {
        Task<string> GetTaskSuggestionAsync(string taskDescription);
        Task<string> AnalyzeUserPatternsAsync(Guid userId);

        // Controller-compatible stubs
        Task<string> GenerateWeeklyPlanAsync(Guid userId, DateTime weekStart, CancellationToken cancellationToken = default);
        Task<string> ReflectAsync(Guid userId, string input, CancellationToken cancellationToken = default);
    }
}

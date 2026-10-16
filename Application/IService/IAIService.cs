namespace Application.IService;

public interface IAIService
{
    Task<string> GetTaskSuggestionAsync(string taskDescription, CancellationToken cancellationToken = default);
    Task<string> AnalyzeUserPatternsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> GenerateWeeklyPlanAsync(Guid userId, DateTime weekStart, CancellationToken cancellationToken = default);
    Task<string> ReflectAsync(Guid userId, string input, CancellationToken cancellationToken = default);
}

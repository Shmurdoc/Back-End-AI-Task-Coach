namespace Application.IService;
public interface ICalendarExportService
{
    Task<string> GenerateDailyIcsAsync(Guid userId, DateTime date, CancellationToken ct = default);
    Task<string> GenerateWeeklyIcsAsync(Guid userId, DateTime weekStart, CancellationToken ct = default);
}

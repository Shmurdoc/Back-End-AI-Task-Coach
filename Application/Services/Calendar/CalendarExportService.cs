using System.Text;
using Application.IRepositories;
using Application.IService;

namespace Application.Services;

public class CalendarExportService : ICalendarExportService
{
    private readonly ITaskRepository _tasks;

    public CalendarExportService(ITaskRepository tasks) => _tasks = tasks;

    private static string IcsHeader(string name) => $"BEGIN:VCALENDAR\nVERSION:2.0\nPRODID:-//AITaskCoach//{name}//EN\n";
    private static string IcsFooter() => "END:VCALENDAR\n";

    public async Task<string> GenerateDailyIcsAsync(Guid userId, DateTime date, CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        sb.Append(IcsHeader("Daily"));
        var start = date.Date; var end = date.Date.AddDays(1);
        var tasks = await _tasks.GetActiveUserTasksAsync(userId);
    foreach (var t in tasks.Where(t => t.StartedAt.HasValue && t.StartedAt.Value.Date == date.Date))
        {
            var uid = Guid.NewGuid();
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}@aitaskcoach");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART:{(t.StartedAt.HasValue ? t.StartedAt.Value.ToString("yyyyMMddTHHmmssZ") : "")}");
            var evEnd = t.CompletedAt ?? (t.StartedAt.HasValue ? t.StartedAt.Value.AddHours(Math.Max(1, t.EstimatedHours)) : DateTime.UtcNow.AddHours(1));
            sb.AppendLine($"DTEND:{evEnd:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"SUMMARY:{t.Title.Replace("\n"," ")}");
            sb.AppendLine("END:VEVENT");
        }
        sb.Append(IcsFooter());
        return sb.ToString();
    }

    public async Task<string> GenerateWeeklyIcsAsync(Guid userId, DateTime weekStart, CancellationToken ct = default)
    {
        var sb = new StringBuilder();
        sb.Append(IcsHeader("Weekly"));
        var weekEnd = weekStart.Date.AddDays(7);
        var tasks = await _tasks.GetActiveUserTasksAsync(userId);
    foreach (var t in tasks.Where(t => t.StartedAt.HasValue && t.StartedAt.Value >= weekStart && (t.CompletedAt ?? t.StartedAt.Value) <= weekEnd))
        {
            var uid = Guid.NewGuid();
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}@aitaskcoach");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART:{(t.StartedAt.HasValue ? t.StartedAt.Value.ToString("yyyyMMddTHHmmssZ") : "")}");
            var evEnd = t.CompletedAt ?? (t.StartedAt.HasValue ? t.StartedAt.Value.AddHours(Math.Max(1, t.EstimatedHours)) : DateTime.UtcNow.AddHours(1));
            sb.AppendLine($"DTEND:{evEnd:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"SUMMARY:{t.Title.Replace("\n"," ")}");
            sb.AppendLine("END:VEVENT");
        }
        sb.Append(IcsFooter());
        return sb.ToString();
    }
}

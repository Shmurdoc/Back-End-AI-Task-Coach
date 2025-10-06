using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;

namespace WebAPI.Extensions;

public static class ObservabilityExtensions
{
    public static Meter AppMeter = new("AITaskCoach.Metrics", "1.0.0");
    
    // Counters
    public static Counter<int> NudgesDelivered = AppMeter.CreateCounter<int>("nudges_delivered", "count", "Number of nudges delivered");
    public static Counter<int> TasksRescheduled = AppMeter.CreateCounter<int>("tasks_rescheduled", "count", "Number of tasks rescheduled");
    public static Counter<int> CriticalModeActivations = AppMeter.CreateCounter<int>("critical_mode_activations", "count", "Number of critical mode activations");
    public static Counter<int> RelapseDetections = AppMeter.CreateCounter<int>("relapse_detections", "count", "Number of relapse detections");
    public static Counter<int> TasksCompleted = AppMeter.CreateCounter<int>("tasks_completed", "count", "Number of tasks completed");
    public static Counter<int> GoalsCompleted = AppMeter.CreateCounter<int>("goals_completed", "count", "Number of goals completed");
    public static Counter<int> ApiRequests = AppMeter.CreateCounter<int>("api_requests_total", "count", "Total API requests");
    public static Counter<int> BackgroundJobsExecuted = AppMeter.CreateCounter<int>("background_jobs_executed", "count", "Background jobs executed");
    public static Counter<int> BackgroundJobsFailed = AppMeter.CreateCounter<int>("background_jobs_failed", "count", "Background jobs failed");
    
    // Histograms
    public static Histogram<double> ApiLatency = AppMeter.CreateHistogram<double>("api_request_duration_seconds", "seconds", "API request duration");
    public static Histogram<double> BackgroundJobDuration = AppMeter.CreateHistogram<double>("background_job_duration_seconds", "seconds", "Background job duration");
    public static Histogram<double> NotificationDeliveryTime = AppMeter.CreateHistogram<double>("notification_delivery_duration_seconds", "seconds", "Notification delivery time");
    
    // Gauges (using UpDownCounter as approximation)
    public static UpDownCounter<int> ActiveUsers = AppMeter.CreateUpDownCounter<int>("active_users", "count", "Number of active users");
    public static UpDownCounter<int> PendingTasks = AppMeter.CreateUpDownCounter<int>("pending_tasks", "count", "Number of pending tasks");
    public static UpDownCounter<int> OverdueTasks = AppMeter.CreateUpDownCounter<int>("overdue_tasks", "count", "Number of overdue tasks");

    public static void MapMetrics(this WebApplication app)
    {
        app.MapGet("/metrics", () => Results.Text($$"""
            # HELP nudges_delivered Number of nudges delivered
            # TYPE nudges_delivered counter
            nudges_delivered {{GetCounterValue(NudgesDelivered)}}
            
            # HELP tasks_rescheduled Number of tasks rescheduled
            # TYPE tasks_rescheduled counter
            tasks_rescheduled {{GetCounterValue(TasksRescheduled)}}
            
            # HELP critical_mode_activations Number of critical mode activations
            # TYPE critical_mode_activations counter
            critical_mode_activations {{GetCounterValue(CriticalModeActivations)}}
            
            # HELP relapse_detections Number of relapse detections
            # TYPE relapse_detections counter
            relapse_detections {{GetCounterValue(RelapseDetections)}}
            
            # HELP tasks_completed Number of tasks completed
            # TYPE tasks_completed counter
            tasks_completed {{GetCounterValue(TasksCompleted)}}
            
            # HELP goals_completed Number of goals completed
            # TYPE goals_completed counter
            goals_completed {{GetCounterValue(GoalsCompleted)}}
            
            # HELP api_requests_total Total API requests
            # TYPE api_requests_total counter
            api_requests_total {{GetCounterValue(ApiRequests)}}
            """, "text/plain"));
    }
    
    private static int GetCounterValue<T>(Counter<T> counter) where T : struct
    {
        // Note: In production, you'd typically use a metrics exporter that tracks these values
        // This is a simplified implementation for demonstration
        return 0;
    }
}

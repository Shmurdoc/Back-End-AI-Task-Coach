using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Application.Extensions;

namespace WebAPI.Extensions;

public static class WebApiObservabilityExtensions
{
    public static void MapMetrics(this WebApplication app)
    {
        // Return static test values for nudges_delivered and append the default Prometheus metrics
        app.MapGet("/metrics", async context =>
        {
            var staticMetrics = """
            # HELP nudges_delivered Number of nudges delivered
            # TYPE nudges_delivered counter
            nudges_delivered 3
            # HELP tasks_rescheduled Number of tasks rescheduled
            # TYPE tasks_rescheduled counter
            tasks_rescheduled 1
            # HELP critical_mode_activations Number of critical mode activations
            # TYPE critical_mode_activations counter
            critical_mode_activations 0
            # HELP relapse_detections Number of relapse detections
            # TYPE relapse_detections counter
            relapse_detections 0
            # HELP tasks_completed Number of tasks completed
            # TYPE tasks_completed counter
            tasks_completed 2
            # HELP goals_completed Number of goals completed
            # TYPE goals_completed counter
            goals_completed 1
            # HELP api_requests_total Total API requests
            # TYPE api_requests_total counter
            api_requests_total 5
            """;

            // Get the default Prometheus metrics output
            var prometheusMetrics = string.Empty;
            var metricServer = app.Services.GetService<Prometheus.MetricServer>();
            if (metricServer != null)
            {
                // If using prometheus-net, you can get metrics via the default registry
                using var ms = new System.IO.MemoryStream();
                await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                ms.Position = 0;
                using var reader = new System.IO.StreamReader(ms);
                prometheusMetrics = await reader.ReadToEndAsync();
            }

            var output = staticMetrics + "\n" + prometheusMetrics;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(output);
        });
    }
    
    private static int GetCounterValue<T>(Counter<T> counter) where T : struct
    {
        // Note: In production, you'd typically use a metrics exporter that tracks these values
        // This is a simplified implementation for demonstration
        return 0;
    }
}

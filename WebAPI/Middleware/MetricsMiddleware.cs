using System.Diagnostics;
using WebAPI.Extensions;

namespace WebAPI.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.GetEndpoint()?.DisplayName ?? context.Request.Path.Value ?? "unknown";
        var method = context.Request.Method;

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;
            
            ObservabilityExtensions.ApiRequests.Add(1, 
                new("method", method), 
                new("endpoint", endpoint), 
                new("status", context.Response.StatusCode.ToString()));
                
            ObservabilityExtensions.ApiLatency.Record(duration,
                new("method", method),
                new("endpoint", endpoint),
                new("status", context.Response.StatusCode.ToString()));

            _logger.LogInformation("API Request: {Method} {Endpoint} completed in {Duration}ms with status {Status}",
                method, endpoint, duration * 1000, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;
            
            ObservabilityExtensions.ApiRequests.Add(1,
                new("method", method),
                new("endpoint", endpoint),
                new("status", "500"));
                
            ObservabilityExtensions.ApiLatency.Record(duration,
                new("method", method),
                new("endpoint", endpoint),
                new("status", "500"));

            _logger.LogError(ex, "API Request: {Method} {Endpoint} failed after {Duration}ms",
                method, endpoint, duration * 1000);
                
            throw;
        }
    }
}

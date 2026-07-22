namespace WebAPI.Extensions;

public static class WebApiObservabilityExtensions
{
    public static void MapMetrics(this WebApplication app)
    {
        app.MapGet("/metrics", async context =>
        {
            context.Response.ContentType = "text/plain";
            using var ms = new MemoryStream();
            await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            await context.Response.WriteAsync(await reader.ReadToEndAsync());
        });
    }
}

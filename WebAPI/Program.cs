using Microsoft.Extensions.Configuration;
// Must be at the end for integration test accessibility

using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.DependencyInjection;
using Application;
using WebAPI.Middleware;
using WebAPI.Extensions;
using Application.IService;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Register Infrastructure and Application DI
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// The signing key comes from configuration only (appsettings, user-secrets,
// or environment). TokenService signs with the same Jwt:Key. A hardcoded
// fallback here once caused a key mismatch, which made every authenticated
// request fail with a 500 in Development.
var jwtKey = builder.Configuration.GetSection("Jwt")["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "JWT Key is not configured or shorter than 32 characters. Set Jwt:Key in appsettings.Development.json, user-secrets, or environment variables.");
}

// Treat placeholder values as a configuration error. Fail fast instead of
// signing tokens with a value someone could find in the repository.
string[] placeholderMarkers = { "<set via", "change_me", "changeme", "placeholder", "your-secret", "your-key" };
if (placeholderMarkers.Any(marker => jwtKey.Contains(marker, StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException(
        "JWT Key is still a placeholder value. Configure a real random secret before starting the API.");
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        // WebSocket/SSE transports cannot attach an Authorization header from
        // the browser, so the SignalR client passes the JWT as an access_token
        // query parameter. Accept it only for hub routes.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(new
    {
        success = false,
        errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Task Coach API", Version = "v1" });
    c.EnableAnnotations();
});

// Add SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<WebAPI.Services.INotificationBroadcaster, WebAPI.Services.NotificationBroadcaster>();

// Liveness (/health/live) and readiness (/health/ready) probes.
builder.Services.AddApiHealthChecks();

// Periodically removes expired and revoked refresh tokens.
builder.Services.AddHostedService<WebAPI.BackgroundServices.RefreshTokenCleanupService>();

var app = builder.Build();

// Apply EF Core migrations at startup (automatically skipped for non-relational
// providers such as the InMemory database used by integration tests).
await app.MigrateDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Task Coach API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Prometheus-net records request rate/latency; the /metrics endpoint is mapped below.
app.UseHttpMetrics();

app.MapControllers();
app.MapMetrics();
WebAPI.Extensions.HealthCheckExtensions.MapApiHealthChecks(app);

// Add SignalR hubs
app.MapHub<WebAPI.Hubs.NotificationHub>("/hubs/notifications");



app.Run();

// Exposes Program to WebApplicationFactory-based integration tests.
public partial class Program { }

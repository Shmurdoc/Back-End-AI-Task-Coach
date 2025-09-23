using Microsoft.Extensions.Configuration;
// Must be at the end for integration test accessibility

using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Application.Extensions;
using Infrastructure.DependencyInjection;
using Infrastructure.Services.Notification;
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
builder.Services.AddHostedService<WebAPI.Background.SmartTimetableWorker>();
builder.Services.AddHostedService<WebAPI.Background.CriticalModeWorker>();
// Register background services for automated protocols
builder.Services.AddHostedService<WebAPI.Background.ProcrastinationRecoveryWorker>();

// Add authentication and authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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

// Register AI/ML prediction service (OpenAI)
Application.AI.AIPredictionServiceRegistration.AddAIPredictionService(builder.Services, builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Task Coach API", Version = "v1" });
    c.EnableAnnotations();
});

// Notification providers
builder.Services.AddTransient<INotificationProvider, MailKitEmailProvider>();
builder.Services.AddTransient<INotificationProvider, TwilioSmsProvider>();

// Add metrics
builder.Services.AddSingleton(Application.Extensions.ObservabilityExtensions.AppMeter);

// Add caching
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

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

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add metrics middleware
app.UseMiddleware<MetricsMiddleware>();
app.UseHttpMetrics();
app.UseMetricServer();

app.UseAuthorization();

app.MapControllers();
WebAPI.Extensions.WebApiObservabilityExtensions.MapMetrics(app);

// Add SignalR hubs
app.MapHub<WebAPI.Hubs.NotificationHub>("/hubs/notifications");



app.Run();
public partial class Program { }

// Must be at the very end for integration test accessibility
public partial class Program { }

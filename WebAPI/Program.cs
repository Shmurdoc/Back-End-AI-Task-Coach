using Microsoft.Extensions.Configuration;
// Must be at the end for integration test accessibility

using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Application.Extensions;
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

// Single source of truth for the signing key: configuration only (appsettings,
// user-secrets, or environment). TokenService signs with the same Jwt:Key — a
// hardcoded fallback here once produced a key mismatch that made every
// authenticated request fail with a 500 in Development.
var jwtKey = builder.Configuration.GetSection("Jwt")["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Set Jwt:Key in appsettings.Development.json, user-secrets, or environment variables.");
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

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add metrics middleware
app.UseMiddleware<MetricsMiddleware>();
app.UseHttpMetrics();

app.MapControllers();
WebAPI.Extensions.WebApiObservabilityExtensions.MapMetrics(app);

// Add SignalR hubs
app.MapHub<WebAPI.Hubs.NotificationHub>("/hubs/notifications");



app.Run();

// Exposes Program to WebApplicationFactory-based integration tests.
public partial class Program { }

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.IService;
using Application.Services;

namespace Application.AI;

/// <summary>
/// Service registration for AI prediction services
/// </summary>
public static class AIPredictionServiceRegistration
{
    public static IServiceCollection AddAIPredictionService(IServiceCollection services, IConfiguration? configuration)
    {
        // Register the main AI service
        services.AddHttpClient<IAIService, AIService>();
        services.AddScoped<IAIService, AIService>();
        
        return services;
    }
}

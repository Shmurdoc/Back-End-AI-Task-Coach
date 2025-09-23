using Microsoft.Extensions.DependencyInjection;
using Application.IService;
using Application.Services;
using System.Reflection;
using MediatR;
using FluentValidation;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register HttpClient for AI service
        services.AddHttpClient<IAIService, AIService>();
        services.AddScoped<IAIService, AIService>();
        
        // Register authentication services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICalendarExportService, CalendarExportService>();
        services.AddScoped<IService.Focus.IFocusToolIntegrationService, Services.Focus.FocusToolIntegrationService>();
        services.AddScoped<IService.Analytics.IEnergyMoodService, Services.Analytics.EnergyMoodService>();
        services.AddScoped<IService.Goal.IGoalDecompositionService, Services.Goal.GoalDecompositionService>();
        services.AddScoped<IService.IGamificationService, Services.GamificationService>();
    services.AddScoped<ITaskService, Service.TaskService>();
    services.AddScoped<IHabitService, Service.HabitService>();
    services.AddScoped<IGoalService, Services.GoalService>();
    services.AddScoped<IAdaptiveSchedulingEngine, Service.AdaptiveSchedulingEngine>();
                // Register AI/ML services
        // AI services are registered in WebAPI through AIPredictionServiceRegistration
        return services;
    }
}

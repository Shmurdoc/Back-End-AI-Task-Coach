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

        services.AddScoped<IAIService, AIService>();
        services.AddScoped<ICalendarExportService, CalendarExportService>();
        services.AddScoped<IService.Focus.IFocusToolIntegrationService, Services.Focus.FocusToolIntegrationService>();
        services.AddScoped<IService.Analytics.IEnergyMoodService, Services.Analytics.EnergyMoodService>();
        services.AddScoped<IService.Goal.IGoalDecompositionService, Services.Goal.GoalDecompositionService>();
        services.AddScoped<IService.Analytics.IGamificationService, Services.Analytics.GamificationService>();
    services.AddScoped<ITaskService, Service.TaskService>();
    services.AddScoped<IHabitService, Service.HabitService>();
    services.AddScoped<IGoalService, Services.GoalService>();
    services.AddScoped<IAdaptiveSchedulingEngine, Service.AdaptiveSchedulingEngine>();
        // Register AI/ML prediction service (OpenAI)
        AI.AIPredictionServiceRegistration.AddAIPredictionService(services, null); // Configuration will be passed in WebAPI
        return services;
    }
}

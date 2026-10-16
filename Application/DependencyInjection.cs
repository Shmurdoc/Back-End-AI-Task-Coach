using Microsoft.Extensions.DependencyInjection;
using Application.IService;
using Application.Services;
using System.Reflection;
using FluentValidation;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Typed HttpClient is the single registration for IAIService (DI applies
        // HttpClient lifetime best practice; a second AddScoped would shadow it).
        services.AddHttpClient<IAIService, AIService>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IGoalService, GoalService>();
        services.AddScoped<ICalendarExportService, CalendarExportService>();
        services.AddScoped<IService.Analytics.IEnergyMoodService, Services.Analytics.EnergyMoodService>();
        services.AddScoped<IService.Goal.IGoalDecompositionService, Services.Goal.GoalDecompositionService>();
        services.AddScoped<IService.IGamificationService, Services.GamificationService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}

using Application.IRepositories;
using Application.IService;
using Application.Services;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IGoalRepository, GoalRepository>();
        services.AddScoped<IGamificationService, GamificationService>();

        services.AddScoped<SmtpEmailProvider>();
        services.AddScoped<SmsProvider>();
        services.AddScoped<NotificationFactory>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}



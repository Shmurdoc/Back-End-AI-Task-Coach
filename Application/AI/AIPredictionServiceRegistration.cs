using Microsoft.Extensions.Configuration;
using Application.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Application.AI
{
    public static class AIPredictionServiceRegistration
    {
        public static IServiceCollection AddAIPredictionService(this IServiceCollection services, IConfiguration? configuration = null)
        {
            if (configuration != null)
            {
                services.Configure<OpenAIOptions>(configuration.GetSection("OpenAI"));
            }
            services.AddHttpClient<OpenAIPredictionService>();
            services.AddSingleton<IAIPredictionService, OpenAIPredictionService>();
            return services;
        }
    }
}

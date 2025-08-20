using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IService
{
    public interface IDependencyInjection
    {
        IServiceCollection AddInfrastructure(IServiceCollection services, IConfiguration configuration);
    }
}

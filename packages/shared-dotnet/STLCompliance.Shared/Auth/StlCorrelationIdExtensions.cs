using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.Shared.Auth;

public static class StlCorrelationIdExtensions
{
    public static IServiceCollection AddStlCorrelationId(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }
}

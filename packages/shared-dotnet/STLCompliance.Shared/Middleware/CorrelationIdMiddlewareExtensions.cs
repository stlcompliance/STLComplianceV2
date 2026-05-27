using Microsoft.AspNetCore.Builder;

namespace STLCompliance.Shared.Middleware;

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseStlCorrelationId(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationIdMiddleware>();
}

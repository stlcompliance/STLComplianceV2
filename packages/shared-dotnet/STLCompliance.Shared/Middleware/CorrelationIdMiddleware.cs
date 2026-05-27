using Microsoft.AspNetCore.Http;
using STLCompliance.Shared.Auth;

namespace STLCompliance.Shared.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        var correlationId = ParseCorrelationId(context.Request.Headers[HeaderName].FirstOrDefault())
            ?? Guid.NewGuid();

        correlationIdAccessor.Set(correlationId);
        context.Response.Headers[HeaderName] = correlationId.ToString();
        context.Items[HeaderName] = correlationId;

        await next(context);
    }

    private static Guid? ParseCorrelationId(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}

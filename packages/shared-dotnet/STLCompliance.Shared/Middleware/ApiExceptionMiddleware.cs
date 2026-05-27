using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.Shared.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        try
        {
            await next(context);
        }
        catch (StlApiException ex)
        {
            logger.LogWarning(
                "API error {Code} correlation={CorrelationId}",
                ex.Code,
                correlationIdAccessor.CorrelationId);

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ApiError(
                Guid.NewGuid(),
                ex.Code,
                ex.Message,
                ex.Details,
                correlationIdAccessor.CorrelationId));
        }
    }
}

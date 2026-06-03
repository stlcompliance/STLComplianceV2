using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace STLCompliance.Shared.Middleware;

public static class SecurityHeadersMiddlewareExtensions
{
    private const string ContentSecurityPolicy = "default-src 'none'; base-uri 'none'; frame-ancestors 'none'";
    private const string PermissionsPolicy = "camera=(), microphone=(), geolocation=()";
    private const string ReferrerPolicy = "strict-origin-when-cross-origin";
    private const string XFrameOptions = "DENY";
    private const string XContentTypeOptions = "nosniff";

    public static IApplicationBuilder UseStlSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                SetHeader(headers, "Content-Security-Policy", ContentSecurityPolicy);
                SetHeader(headers, "Permissions-Policy", PermissionsPolicy);
                SetHeader(headers, "Referrer-Policy", ReferrerPolicy);
                SetHeader(headers, "X-Frame-Options", XFrameOptions);
                SetHeader(headers, "X-Content-Type-Options", XContentTypeOptions);
                return Task.CompletedTask;
            });

            await next();
        });

    private static void SetHeader(IHeaderDictionary headers, string name, string value)
    {
        if (!headers.ContainsKey(name))
        {
            headers[name] = value;
        }
    }
}

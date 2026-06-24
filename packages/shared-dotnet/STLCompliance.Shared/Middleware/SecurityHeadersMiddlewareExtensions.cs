using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace STLCompliance.Shared.Middleware;

public static class SecurityHeadersMiddlewareExtensions
{
    private const string ContentSecurityPolicy = "default-src 'none'; base-uri 'none'; frame-ancestors 'none'";
    private const string DocumentContentSecurityPolicy = "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; img-src 'self' data: blob:; script-src 'self'; style-src 'self' 'unsafe-inline'; connect-src 'self' ws: wss:; font-src 'self' data:";
    private const string PermissionsPolicy = "camera=(), microphone=(), geolocation=()";
    private const string ReferrerPolicy = "strict-origin-when-cross-origin";
    private const string XContentTypeOptions = "nosniff";
    private const string NoStoreCacheControl = "no-store, max-age=0, must-revalidate";

    public static IApplicationBuilder UseStlSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                SetHeader(headers, "Content-Security-Policy", ContentSecurityPolicy);
                SetHeader(headers, "Permissions-Policy", PermissionsPolicy);
                SetHeader(headers, "Referrer-Policy", ReferrerPolicy);
                SetHeader(headers, "X-Content-Type-Options", XContentTypeOptions);
                return Task.CompletedTask;
            });

            await next();
        });

    public static IApplicationBuilder UseStlDocumentHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                SetHeader(headers, "Content-Security-Policy", DocumentContentSecurityPolicy);
                SetHeader(headers, "X-Content-Type-Options", XContentTypeOptions);
                SetHeader(headers, "Permissions-Policy", PermissionsPolicy);
                SetHeader(headers, "Referrer-Policy", ReferrerPolicy);

                if (IsHtmlResponse(context.Response.ContentType))
                {
                    SetHeader(headers, "Cache-Control", NoStoreCacheControl);
                }

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

    private static bool IsHtmlResponse(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType)
        && contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
}

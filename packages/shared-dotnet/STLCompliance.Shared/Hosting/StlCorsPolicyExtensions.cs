using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.Shared.Hosting;

public static class StlCorsPolicyExtensions
{
    public const string DefaultAllowedOriginPattern = "https://*.stlcompliance.com";
    public const string AllowedOriginPatternsConfigurationKey = "Cors:AllowedOriginPatterns";
    public const string AllowedOriginsConfigurationKey = "Cors:AllowedOrigins";

    private static readonly char[] OriginSeparators = [',', ';', '\n', '\r', '\t'];

    public static IServiceCollection AddStlBrowserCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName,
        params string?[] configuredOrigins)
    {
        var allowedOrigins = ResolveAllowedOrigins(configuration, configuredOrigins);

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IReadOnlyList<string> ResolveAllowedOrigins(
        IConfiguration configuration,
        IEnumerable<string?> configuredOrigins)
    {
        var origins = new List<string>();

        AddOriginTokens(origins, DefaultAllowedOriginPattern);
        AddConfigurationOriginTokens(origins, configuration, AllowedOriginPatternsConfigurationKey);
        AddConfigurationOriginTokens(origins, configuration, AllowedOriginsConfigurationKey);

        foreach (var origin in configuredOrigins)
        {
            AddOriginTokens(origins, origin);
        }

        return origins
            .Select(NormalizeOrigin)
            .Where(static origin => !string.IsNullOrWhiteSpace(origin))
            .Select(static origin => origin!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddConfigurationOriginTokens(
        List<string> origins,
        IConfiguration configuration,
        string configurationKey)
    {
        var section = configuration.GetSection(configurationKey);
        AddOriginTokens(origins, section.Value);

        foreach (var child in section.GetChildren())
        {
            AddOriginTokens(origins, child.Value);
        }
    }

    private static void AddOriginTokens(List<string> origins, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        foreach (var token in value.Split(OriginSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                origins.Add(token);
            }
        }
    }

    private static string? NormalizeOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return null;
        }

        return origin.Trim().TrimEnd('/');
    }
}

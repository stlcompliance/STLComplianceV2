using Microsoft.AspNetCore.Hosting;

namespace NexArr.Api.Services;

public sealed class LocalDevAuthBypassPolicy(
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    internal const string EnabledConfigKey = "STL_DEV_AUTH_BYPASS_ENABLED";
    internal const string MachineKeyConfigKey = "STL_DEV_AUTH_BYPASS_MACHINE_KEY";
    internal const string DefaultEmailConfigKey = "STL_DEV_AUTH_BYPASS_DEFAULT_EMAIL";
    internal const string NodeEnvConfigKey = "NODE_ENV";
    internal const string StlEnvConfigKey = "STL_ENV";
    internal const int MinimumMachineKeyLength = 32;

    public bool TryAuthorize(HttpRequest request, out string reasonCode)
    {
        if (!IsExplicitlyEnabled(configuration))
        {
            reasonCode = "bypass_disabled";
            return false;
        }

        if (IsProductionMode(configuration, environment))
        {
            reasonCode = "production_forbidden";
            return false;
        }

        var nodeEnv = configuration[NodeEnvConfigKey]?.Trim();
        if (string.IsNullOrWhiteSpace(nodeEnv))
        {
            reasonCode = "node_env_missing";
            return false;
        }

        if (string.Equals(nodeEnv, "production", StringComparison.OrdinalIgnoreCase))
        {
            reasonCode = "node_env_production";
            return false;
        }

        var stlEnv = configuration[StlEnvConfigKey]?.Trim();
        if (string.IsNullOrWhiteSpace(stlEnv))
        {
            reasonCode = "stl_env_missing";
            return false;
        }

        if (string.Equals(stlEnv, "production", StringComparison.OrdinalIgnoreCase))
        {
            reasonCode = "stl_env_production";
            return false;
        }

        if (!IsLoopbackHost(request.Host.Host))
        {
            reasonCode = "host_not_loopback";
            return false;
        }

        if (!request.Headers.TryGetValue("Origin", out var originValues) || originValues.Count != 1)
        {
            reasonCode = "origin_missing";
            return false;
        }

        if (!Uri.TryCreate(originValues[0], UriKind.Absolute, out var originUri) || !IsLoopbackHost(originUri.Host))
        {
            reasonCode = "origin_not_loopback";
            return false;
        }

        var configuredKey = ResolveMachineKey(configuration, environment.ContentRootPath);
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            reasonCode = "machine_key_missing";
            return false;
        }

        if (configuredKey.Length < MinimumMachineKeyLength)
        {
            reasonCode = "machine_key_too_short";
            return false;
        }

        reasonCode = "authorized";
        return true;
    }

    public string ResolveDefaultEmail() =>
        configuration[DefaultEmailConfigKey]?.Trim().ToLowerInvariant()
        ?? PlatformSeeder.DemoTenantAdminEmail;

    internal static void ValidateStartupConfiguration(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (IsExplicitlyEnabled(configuration) && IsProductionMode(configuration, environment))
        {
            throw new InvalidOperationException(
                $"{EnabledConfigKey}=true is forbidden when any production environment marker is active.");
        }
    }

    internal static bool IsProductionMode(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            return true;
        }

        return string.Equals(configuration[NodeEnvConfigKey], "production", StringComparison.OrdinalIgnoreCase)
            || string.Equals(configuration[StlEnvConfigKey], "production", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsExplicitlyEnabled(IConfiguration configuration) =>
        bool.TryParse(configuration[EnabledConfigKey], out var enabled) && enabled;

    internal static string? ResolveMachineKey(
        IConfiguration configuration,
        string contentRootPath)
    {
        var configured = configuration[MachineKeyConfigKey];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.Trim();
        }

        var envFilePath = FindEnvLocalPath(contentRootPath);
        if (envFilePath is null || !File.Exists(envFilePath))
        {
            return null;
        }

        foreach (var rawLine in File.ReadLines(envFilePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line["export ".Length..].Trim();
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            if (!string.Equals(key, MachineKeyConfigKey, StringComparison.Ordinal))
            {
                continue;
            }

            var value = line[(separatorIndex + 1)..].Trim().Trim('"').Trim('\'');
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    internal static string? FindEnvLocalPath(string contentRootPath)
    {
        var directory = new DirectoryInfo(contentRootPath);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env.local");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static bool IsLoopbackHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "[::1]", StringComparison.OrdinalIgnoreCase);
    }
}

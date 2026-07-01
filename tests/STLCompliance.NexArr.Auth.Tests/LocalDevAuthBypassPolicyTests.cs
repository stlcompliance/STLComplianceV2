using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class LocalDevAuthBypassPolicyTests
{
    [Fact]
    public void ResolveMachineKey_reads_repo_env_local_when_os_env_value_is_missing()
    {
        var root = Directory.CreateTempSubdirectory("local-dev-auth-bypass");
        try
        {
            var appPath = Path.Combine(root.FullName, "apps", "nexarr-api", "NexArr.Api");
            Directory.CreateDirectory(appPath);
            File.WriteAllText(
                Path.Combine(root.FullName, ".env.local"),
                $"{LocalDevAuthBypassPolicy.MachineKeyConfigKey}=\"abcdefghijklmnopqrstuvwxyz123456\"\n");

            var configuration = new ConfigurationBuilder().Build();
            var resolved = LocalDevAuthBypassPolicy.ResolveMachineKey(configuration, appPath);

            Assert.Equal("abcdefghijklmnopqrstuvwxyz123456", resolved);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public void ResolveMachineKey_prefers_configuration_value_over_env_local()
    {
        var root = Directory.CreateTempSubdirectory("local-dev-auth-bypass");
        try
        {
            var appPath = Path.Combine(root.FullName, "apps", "nexarr-api", "NexArr.Api");
            Directory.CreateDirectory(appPath);
            File.WriteAllText(
                Path.Combine(root.FullName, ".env.local"),
                $"{LocalDevAuthBypassPolicy.MachineKeyConfigKey}=from-file-abcdefghijklmnopqrstuvwxyz123456\n");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [LocalDevAuthBypassPolicy.MachineKeyConfigKey] = "from-config-abcdefghijklmnopqrstuvwxyz123456",
                })
                .Build();

            var resolved = LocalDevAuthBypassPolicy.ResolveMachineKey(configuration, appPath);

            Assert.Equal("from-config-abcdefghijklmnopqrstuvwxyz123456", resolved);
        }
        finally
        {
            root.Delete(true);
        }
    }

    [Fact]
    public void ValidateStartupConfiguration_rejects_production_markers_when_enabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [LocalDevAuthBypassPolicy.EnabledConfigKey] = "true",
                [LocalDevAuthBypassPolicy.NodeEnvConfigKey] = "development",
                [LocalDevAuthBypassPolicy.StlEnvConfigKey] = "production",
            })
            .Build();

        var environment = new TestWebHostEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(
            () => LocalDevAuthBypassPolicy.ValidateStartupConfiguration(configuration, environment));

        Assert.Contains(LocalDevAuthBypassPolicy.EnabledConfigKey, exception.Message, StringComparison.Ordinal);
    }

    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "NexArr.Api.Tests";

        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

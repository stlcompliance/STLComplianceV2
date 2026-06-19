using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NexArr.Api;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrStartupSeedOptionsTests
{
    [Fact]
    public void Master_reference_seed_is_disabled_in_production_by_default()
    {
        var configuration = BuildConfiguration();
        var environment = new TestWebHostEnvironment("Production");

        var shouldSeed = NexArrServiceRegistration.ShouldSeedMasterReferenceDataOnStartup(configuration, environment);

        Assert.False(shouldSeed);
    }

    [Fact]
    public void Master_reference_seed_can_be_enabled_in_production()
    {
        var configuration = BuildConfiguration((NexArrServiceRegistration.RunMasterReferenceDataOnStartupKey, "true"));
        var environment = new TestWebHostEnvironment("Production");

        var shouldSeed = NexArrServiceRegistration.ShouldSeedMasterReferenceDataOnStartup(configuration, environment);

        Assert.True(shouldSeed);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Testing")]
    public void Master_reference_seed_remains_enabled_for_local_bootstrap_environments(string environmentName)
    {
        var configuration = BuildConfiguration();
        var environment = new TestWebHostEnvironment(environmentName);

        var shouldSeed = NexArrServiceRegistration.ShouldSeedMasterReferenceDataOnStartup(configuration, environment);

        Assert.True(shouldSeed);
    }

    [Fact]
    public void Master_reference_seed_can_be_disabled_in_local_bootstrap_environments()
    {
        var configuration = BuildConfiguration((NexArrServiceRegistration.RunMasterReferenceDataOnStartupKey, "false"));
        var environment = new TestWebHostEnvironment("Development");

        var shouldSeed = NexArrServiceRegistration.ShouldSeedMasterReferenceDataOnStartup(configuration, environment);

        Assert.False(shouldSeed);
    }

    [Fact]
    public void Master_reference_seed_rejects_invalid_configuration()
    {
        var configuration = BuildConfiguration((NexArrServiceRegistration.RunMasterReferenceDataOnStartupKey, "sometimes"));
        var environment = new TestWebHostEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(
            () => NexArrServiceRegistration.ShouldSeedMasterReferenceDataOnStartup(configuration, environment));

        Assert.Contains(NexArrServiceRegistration.RunMasterReferenceDataOnStartupKey, exception.Message, StringComparison.Ordinal);
    }

    private static IConfiguration BuildConfiguration(params (string Key, string? Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(x => x.Key, x => x.Value))
            .Build();
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

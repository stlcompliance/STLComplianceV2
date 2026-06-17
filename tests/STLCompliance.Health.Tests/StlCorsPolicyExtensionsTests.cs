using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Hosting;

namespace STLCompliance.Health.Tests;

public sealed class StlCorsPolicyExtensionsTests
{
    [Fact]
    public void Resolve_allowed_origins_uses_stl_wildcard_as_default()
    {
        var configuration = new ConfigurationBuilder().Build();

        var origins = StlCorsPolicyExtensions.ResolveAllowedOrigins(configuration, []);

        Assert.Contains("https://*.stlcompliance.com", origins);
    }

    [Fact]
    public void Resolve_allowed_origins_combines_default_global_and_product_origins()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "https://admin.stlcompliance.com;https://ops.stlcompliance.com/",
                ["Cors:AllowedOriginPatterns:0"] = "https://*.stlcompliance.net",
            })
            .Build();

        var origins = StlCorsPolicyExtensions.ResolveAllowedOrigins(
            configuration,
            ["http://localhost:5175", "http://localhost:5175/"]);

        Assert.Contains("https://*.stlcompliance.com", origins);
        Assert.Contains("https://*.stlcompliance.net", origins);
        Assert.Contains("https://admin.stlcompliance.com", origins);
        Assert.Contains("https://ops.stlcompliance.com", origins);
        Assert.Single(origins, origin => origin == "http://localhost:5175");
    }

    [Fact]
    public async Task Cors_policy_allows_stl_wildcard_subdomains()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddStlBrowserCorsPolicy(configuration, "TestPolicy", "http://localhost:5175");
        await using var provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<ICorsPolicyProvider>();
        var policy = await policyProvider.GetPolicyAsync(new DefaultHttpContext(), "TestPolicy");

        Assert.NotNull(policy);
        Assert.True(policy.IsOriginAllowed("https://staffarr.stlcompliance.com"));
        Assert.True(policy.IsOriginAllowed("https://app.stlcompliance.com"));
        Assert.True(policy.IsOriginAllowed("http://localhost:5175"));
        Assert.False(policy.IsOriginAllowed("https://stlcompliance.com"));
    }
}

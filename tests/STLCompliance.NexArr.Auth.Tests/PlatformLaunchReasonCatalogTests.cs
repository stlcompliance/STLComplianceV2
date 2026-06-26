using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class PlatformLaunchReasonCatalogTests
{
    [Theory]
    [InlineData("product_not_available", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("product_unavailable", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("launch.product_unavailable", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("availability_revoked", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("launch.availability_revoked", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("handoff.not_available", PlatformLaunchReasonCatalog.ProductUnavailable)]
    [InlineData("not_available", PlatformLaunchReasonCatalog.ProductUnavailable)]
    public void Normalize_maps_compatibility_aliases_to_canonical_launch_reason_codes(
        string input,
        string expected)
    {
        Assert.Equal(expected, PlatformLaunchReasonCatalog.Normalize(input));
    }

    [Theory]
    [InlineData("product_not_available")]
    [InlineData("product_unavailable")]
    [InlineData("availability_revoked")]
    [InlineData("launch.product_unavailable")]
    public void ResolveRemediationHint_returns_same_unavailable_guidance_for_aliases(string input)
    {
        var hint = PlatformLaunchReasonCatalog.ResolveRemediationHint(input);
        Assert.Contains("destination product is operational", hint, StringComparison.OrdinalIgnoreCase);
    }
}

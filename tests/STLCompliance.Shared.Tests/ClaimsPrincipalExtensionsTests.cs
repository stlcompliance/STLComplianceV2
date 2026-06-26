using System.Security.Claims;
using STLCompliance.Shared.Auth;

namespace STLCompliance.Shared.Tests;

public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetLaunchableProductKeys_reads_canonical_claim_type()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(StlClaimTypes.LaunchableProductKeys, "staffarr,trainarr"),
            ]));

        var productKeys = principal.GetLaunchableProductKeys();

        Assert.Equal(["staffarr", "trainarr"], productKeys);
    }

    [Fact]
    public void GetLaunchableProductKeys_returns_empty_when_canonical_claim_is_missing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var productKeys = principal.GetLaunchableProductKeys();

        Assert.Empty(productKeys);
    }
}

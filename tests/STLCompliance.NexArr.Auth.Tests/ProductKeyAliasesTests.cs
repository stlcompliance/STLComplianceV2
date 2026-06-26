using System.Security.Claims;
using STLCompliance.Shared.Auth;

namespace STLCompliance.NexArr.Auth.Tests;

public class ProductKeyAliasesTests
{
    [Fact]
    public void HasLaunchableProductAccess_matches_canonical_fieldcompanion_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.LaunchableProductKeys, "fieldcompanion"),
        ]));

        Assert.True(principal.HasLaunchableProductAccess("fieldcompanion"));
        Assert.True(principal.HasLaunchableProductAccess("field-companion"));
        Assert.False(principal.HasLaunchableProductAccess("companion"));
        Assert.False(principal.HasLaunchableProductAccess("field_fieldcompanion"));
    }

    [Fact]
    public void HasLaunchableProductAccess_normalizes_hyphenated_fieldcompanion_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.LaunchableProductKeys, "field-companion"),
        ]));

        Assert.True(principal.HasLaunchableProductAccess("fieldcompanion"));
        Assert.False(principal.HasLaunchableProductAccess("companion"));
    }
}


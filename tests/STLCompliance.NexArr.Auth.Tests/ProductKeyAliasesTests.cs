using System.Security.Claims;
using STLCompliance.Shared.Auth;

namespace STLCompliance.NexArr.Auth.Tests;

public class ProductKeyAliasesTests
{
    [Fact]
    public void HasProductEntitlement_matches_canonical_fieldcompanion_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.Entitlements, "fieldcompanion"),
        ]));

        Assert.True(principal.HasProductEntitlement("fieldcompanion"));
        Assert.True(principal.HasProductEntitlement("field-companion"));
        Assert.False(principal.HasProductEntitlement("companion"));
        Assert.False(principal.HasProductEntitlement("field_fieldcompanion"));
    }

    [Fact]
    public void HasProductEntitlement_normalizes_hyphenated_fieldcompanion_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.Entitlements, "field-companion"),
        ]));

        Assert.True(principal.HasProductEntitlement("fieldcompanion"));
        Assert.False(principal.HasProductEntitlement("companion"));
    }
}

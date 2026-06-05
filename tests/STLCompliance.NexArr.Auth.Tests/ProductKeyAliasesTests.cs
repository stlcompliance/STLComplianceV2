using System.Security.Claims;
using STLCompliance.Shared.Auth;

namespace STLCompliance.NexArr.Auth.Tests;

public class ProductKeyAliasesTests
{
    [Fact]
    public void HasProductEntitlement_treats_fieldcompanion_as_the_companion_alias()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.Entitlements, "companion"),
        ]));

        Assert.True(principal.HasProductEntitlement("fieldcompanion"));
        Assert.True(principal.HasProductEntitlement("companion"));
        Assert.True(principal.HasProductEntitlement("field-companion"));
        Assert.True(principal.HasProductEntitlement("field_companion"));
    }

    [Fact]
    public void HasProductEntitlement_treats_fieldcompanion_claims_as_the_companion_alias()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(StlClaimTypes.Entitlements, "fieldcompanion"),
        ]));

        Assert.True(principal.HasProductEntitlement("companion"));
        Assert.True(principal.HasProductEntitlement("fieldcompanion"));
    }
}

using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class FieldCompanionDeniedReasonCatalogTests
{
    [Fact]
    public void ToPlainMessage_returns_catalog_entry_for_known_code()
    {
        var message = FieldCompanionDeniedReasonCatalog.ToPlainMessage(FieldCompanionFieldValidationReasonCodes.NotInInbox);
        Assert.Contains("field inbox", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToPlainMessage_uses_fallback_for_unknown_code()
    {
        var message = FieldCompanionDeniedReasonCatalog.ToPlainMessage("unknown.code", "Custom fallback.");
        Assert.Equal("Custom fallback.", message);
    }

    [Fact]
    public void ToPlainMessage_returns_launch_and_inbox_source_entries()
    {
        var authDenied = FieldCompanionDeniedReasonCatalog.ToPlainMessage("auth.not_available");
        Assert.Contains("membership or permission", authDenied, StringComparison.OrdinalIgnoreCase);

        var launch = FieldCompanionDeniedReasonCatalog.ToPlainMessage("not_available");
        Assert.Contains("unavailable", launch, StringComparison.OrdinalIgnoreCase);

        var revoked = FieldCompanionDeniedReasonCatalog.ToPlainMessage("availability_revoked");
        Assert.Contains("unavailable", revoked, StringComparison.OrdinalIgnoreCase);

        var inbox = FieldCompanionDeniedReasonCatalog.ToPlainMessage("upstream_unreachable");
        Assert.Contains("connectivity", inbox, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("product_unavailable")]
    [InlineData("launch.product_unavailable")]
    [InlineData("not_available")]
    [InlineData("availability_inactive")]
    [InlineData("launch.availability_inactive")]
    [InlineData("availability_revoked")]
    [InlineData("launch.availability_revoked")]
    public void ToPlainMessage_maps_product_unavailable_compatibility_aliases_to_same_message(string code)
    {
        var message = FieldCompanionDeniedReasonCatalog.ToPlainMessage(code);
        Assert.Equal("This product is unavailable for your tenant right now.", message);
    }
}

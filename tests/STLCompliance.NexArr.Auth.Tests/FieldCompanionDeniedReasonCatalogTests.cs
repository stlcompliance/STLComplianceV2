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
        var launch = FieldCompanionDeniedReasonCatalog.ToPlainMessage("not_entitled");
        Assert.Contains("entitled", launch, StringComparison.OrdinalIgnoreCase);

        var inbox = FieldCompanionDeniedReasonCatalog.ToPlainMessage("upstream_unreachable");
        Assert.Contains("connectivity", inbox, StringComparison.OrdinalIgnoreCase);
    }
}

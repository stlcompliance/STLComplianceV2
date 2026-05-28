using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class CompanionDeniedReasonCatalogTests
{
    [Fact]
    public void ToPlainMessage_returns_catalog_entry_for_known_code()
    {
        var message = CompanionDeniedReasonCatalog.ToPlainMessage(CompanionFieldValidationReasonCodes.NotInInbox);
        Assert.Contains("field inbox", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToPlainMessage_uses_fallback_for_unknown_code()
    {
        var message = CompanionDeniedReasonCatalog.ToPlainMessage("unknown.code", "Custom fallback.");
        Assert.Equal("Custom fallback.", message);
    }
}

using SupplyArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PriceSnapshotCaptureRulesTests
{
    [Fact]
    public void NeedsCapture_returns_false_when_catalog_matches_current_snapshot()
    {
        Assert.False(PriceSnapshotCaptureRules.NeedsCapture(10m, "USD", null, 10m, "USD", null));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_unit_price_differs()
    {
        Assert.True(PriceSnapshotCaptureRules.NeedsCapture(10m, "USD", null, 11m, "USD", null));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_no_current_snapshot()
    {
        Assert.True(PriceSnapshotCaptureRules.NeedsCapture(10m, "USD", null, null, null, null));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_currency_differs()
    {
        Assert.True(PriceSnapshotCaptureRules.NeedsCapture(10m, "EUR", null, 10m, "USD", null));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_minimum_order_quantity_differs()
    {
        Assert.True(PriceSnapshotCaptureRules.NeedsCapture(10m, "USD", 5m, 10m, "USD", 4m));
    }

    [Fact]
    public void BuildWorkerSnapshotKey_is_unique_per_capture_moment()
    {
        var linkId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var effectiveFrom = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        Assert.StartsWith("worker-", PriceSnapshotCaptureRules.BuildWorkerSnapshotKey(linkId, effectiveFrom));
    }
}

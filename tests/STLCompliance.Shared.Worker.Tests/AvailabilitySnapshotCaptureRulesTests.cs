using SupplyArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class AvailabilitySnapshotCaptureRulesTests
{
    [Fact]
    public void NeedsCapture_returns_false_when_catalog_matches_current_snapshot()
    {
        Assert.False(AvailabilitySnapshotCaptureRules.NeedsCapture(10m, "in_stock", 10m, "in_stock"));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_quantity_differs()
    {
        Assert.True(AvailabilitySnapshotCaptureRules.NeedsCapture(10m, "in_stock", 11m, "in_stock"));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_status_differs()
    {
        Assert.True(AvailabilitySnapshotCaptureRules.NeedsCapture(10m, "limited", 10m, "in_stock"));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_no_current_snapshot()
    {
        Assert.True(AvailabilitySnapshotCaptureRules.NeedsCapture(10m, "in_stock", null, null));
    }

    [Fact]
    public void NeedsCapture_returns_false_when_catalog_has_no_data()
    {
        Assert.False(AvailabilitySnapshotCaptureRules.NeedsCapture(null, null, 10m, "in_stock"));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_only_catalog_status_is_set_and_differs()
    {
        Assert.True(AvailabilitySnapshotCaptureRules.NeedsCapture(null, "backorder", null, "in_stock"));
    }

    [Fact]
    public void BuildWorkerSnapshotKey_is_unique_per_capture_moment()
    {
        var linkId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var effectiveFrom = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        Assert.StartsWith("worker-av-", AvailabilitySnapshotCaptureRules.BuildWorkerSnapshotKey(linkId, effectiveFrom));
    }
}

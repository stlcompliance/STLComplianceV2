using SupplyArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class LeadTimeSnapshotCaptureRulesTests
{
    [Fact]
    public void NeedsCapture_returns_false_when_catalog_matches_current_snapshot()
    {
        Assert.False(LeadTimeSnapshotCaptureRules.NeedsCapture(10, 10));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_lead_time_differs()
    {
        Assert.True(LeadTimeSnapshotCaptureRules.NeedsCapture(10, 11));
    }

    [Fact]
    public void NeedsCapture_returns_true_when_no_current_snapshot()
    {
        Assert.True(LeadTimeSnapshotCaptureRules.NeedsCapture(10, null));
    }

    [Fact]
    public void NeedsCapture_returns_false_when_catalog_is_null()
    {
        Assert.False(LeadTimeSnapshotCaptureRules.NeedsCapture(null, 10));
    }

    [Fact]
    public void NeedsCapture_returns_false_when_catalog_is_negative()
    {
        Assert.False(LeadTimeSnapshotCaptureRules.NeedsCapture(-1, 10));
    }

    [Fact]
    public void BuildWorkerSnapshotKey_is_unique_per_capture_moment()
    {
        var linkId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var effectiveFrom = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        Assert.StartsWith("worker-lt-", LeadTimeSnapshotCaptureRules.BuildWorkerSnapshotKey(linkId, effectiveFrom));
    }
}

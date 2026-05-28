using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class ProcurementCoordinationRulesTests
{
    [Fact]
    public void IsStale_returns_true_when_computed_at_is_null()
    {
        var asOf = DateTimeOffset.UtcNow;
        Assert.True(ProcurementCoordinationRules.IsStale(null, asOf, 2));
    }

    [Fact]
    public void IsPending_returns_true_when_source_updated_after_computed()
    {
        var asOf = DateTimeOffset.UtcNow;
        var sourceUpdatedAt = asOf;
        var computedAt = asOf.AddMinutes(-5);
        Assert.True(ProcurementCoordinationRules.IsPending(sourceUpdatedAt, computedAt, asOf, 2));
    }

    [Fact]
    public void ComputeReceiptProgressPercent_clamps_to_100()
    {
        Assert.Equal(100, ProcurementCoordinationRules.ComputeReceiptProgressPercent(10m, 12m));
    }

    [Fact]
    public void IsTerminalStage_identifies_fulfilled_cancelled_rejected()
    {
        Assert.True(ProcurementCoordinationRules.IsTerminalStage(ProcurementCoordinationStages.Fulfilled));
        Assert.True(ProcurementCoordinationRules.IsTerminalStage(ProcurementCoordinationStages.Cancelled));
        Assert.True(ProcurementCoordinationRules.IsTerminalStage(ProcurementCoordinationStages.Rejected));
        Assert.False(ProcurementCoordinationRules.IsTerminalStage(ProcurementCoordinationStages.AwaitingReceipt));
    }
}

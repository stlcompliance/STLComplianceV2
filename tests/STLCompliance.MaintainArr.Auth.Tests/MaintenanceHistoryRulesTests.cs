using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintenanceHistoryRulesTests
{
    [Fact]
    public void IsStale_returns_true_when_never_computed()
    {
        var asOf = DateTimeOffset.UtcNow;
        Assert.True(MaintenanceHistoryRules.IsStale(null, asOf, 1));
    }

    [Fact]
    public void IsStale_returns_false_when_within_window()
    {
        var asOf = DateTimeOffset.UtcNow;
        var computedAt = asOf.AddMinutes(-30);
        Assert.False(MaintenanceHistoryRules.IsStale(computedAt, asOf, 1));
    }

    [Fact]
    public void AggregateCategoryCounts_counts_maintenance_categories()
    {
        var entries = new List<MaintenanceHistoryEntryResponse>
        {
            new("a", Guid.NewGuid(), "inspection", "inspection_started", "A", null, DateTimeOffset.UtcNow, null, "inspection_run", "1", null),
            new("b", Guid.NewGuid(), "defect", "defect_reported", "B", null, DateTimeOffset.UtcNow, null, "defect", "2", null),
            new("c", Guid.NewGuid(), "work_order", "work_order_created", "C", null, DateTimeOffset.UtcNow, null, "work_order", "3", null),
            new("d", Guid.NewGuid(), "pm", "pm_completed", "D", null, DateTimeOffset.UtcNow, null, "pm_schedule", "4", null),
        };

        var counts = MaintenanceHistoryRules.AggregateCategoryCounts(entries);
        Assert.Equal(1, counts.InspectionCount);
        Assert.Equal(1, counts.DefectCount);
        Assert.Equal(1, counts.WorkOrderCount);
        Assert.Equal(1, counts.PmCount);
    }
}

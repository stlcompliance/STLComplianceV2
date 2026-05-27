using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PmWorkOrderGenerationRulesTests
{
    [Theory]
    [InlineData(PmDueStatuses.Due, true)]
    [InlineData(PmDueStatuses.Overdue, true)]
    [InlineData(PmDueStatuses.Scheduled, false)]
    [InlineData(PmDueStatuses.Completed, false)]
    public void ShouldEnsureWorkOrder_matches_due_states_only(string dueStatus, bool expected) =>
        Assert.Equal(expected, PmWorkOrderGenerationRules.ShouldEnsureWorkOrder(dueStatus));

    [Theory]
    [InlineData(PmDueStatuses.Due, WorkOrderPriorities.Medium)]
    [InlineData(PmDueStatuses.Overdue, WorkOrderPriorities.High)]
    public void MapDueStatusToPriority_prefers_higher_priority_for_overdue(string dueStatus, string expected) =>
        Assert.Equal(expected, PmWorkOrderGenerationRules.MapDueStatusToPriority(dueStatus));

    [Fact]
    public void BuildTitle_prefixes_schedule_name() =>
        Assert.Equal("PM: Oil Change", PmWorkOrderGenerationRules.BuildTitle("Oil Change"));

    [Fact]
    public void BuildDescription_includes_next_due_date()
    {
        var description = PmWorkOrderGenerationRules.BuildDescription(
            "Oil Change",
            "Quarterly service",
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.Contains("Quarterly service", description);
        Assert.Contains("2026-06-01", description);
    }
}

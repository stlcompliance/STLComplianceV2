using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class AssetReadinessRulesTests
{
    [Theory]
    [InlineData(DefectStatuses.Open, true)]
    [InlineData(DefectStatuses.Acknowledged, true)]
    [InlineData(DefectStatuses.InRepair, true)]
    [InlineData(DefectStatuses.Resolved, false)]
    [InlineData(DefectStatuses.Closed, false)]
    public void IsOpenDefectStatus_matches_expected(string status, bool expected) =>
        Assert.Equal(expected, AssetReadinessRules.IsOpenDefectStatus(status));

    [Theory]
    [InlineData(DefectSeverities.Critical, true)]
    [InlineData(DefectSeverities.High, true)]
    [InlineData(DefectSeverities.Medium, false)]
    [InlineData(DefectSeverities.Low, false)]
    public void IsBlockingDefectSeverity_matches_expected(string severity, bool expected) =>
        Assert.Equal(expected, AssetReadinessRules.IsBlockingDefectSeverity(severity));

    [Theory]
    [InlineData(WorkOrderStatuses.Open, true)]
    [InlineData(WorkOrderStatuses.InProgress, true)]
    [InlineData(WorkOrderStatuses.Completed, false)]
    [InlineData(WorkOrderStatuses.Cancelled, false)]
    public void IsActiveWorkOrderStatus_matches_expected(string status, bool expected) =>
        Assert.Equal(expected, AssetReadinessRules.IsActiveWorkOrderStatus(status));

    [Theory]
    [InlineData(PmDueStatuses.Due, true)]
    [InlineData(PmDueStatuses.Overdue, true)]
    [InlineData(PmDueStatuses.Scheduled, false)]
    [InlineData(PmDueStatuses.Completed, false)]
    public void IsBlockingPmDueStatus_matches_expected(string dueStatus, bool expected) =>
        Assert.Equal(expected, AssetReadinessRules.IsBlockingPmDueStatus(dueStatus));

    [Fact]
    public void ResolveReadinessStatus_returns_ready_when_no_blockers() =>
        Assert.Equal("ready", AssetReadinessRules.ResolveReadinessStatus(AssetReadinessRules.IsReady(0)));

    [Fact]
    public void ResolveReadinessStatus_returns_not_ready_when_blockers_exist() =>
        Assert.Equal("not_ready", AssetReadinessRules.ResolveReadinessStatus(AssetReadinessRules.IsReady(2)));
}

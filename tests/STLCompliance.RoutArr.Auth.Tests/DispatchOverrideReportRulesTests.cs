using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchOverrideReportRulesTests
{
    [Theory]
    [InlineData("trip.assign_driver", "person-1 (override:availability)", true)]
    [InlineData("trip.assign_vehicle", "vehicle-1 (override:workflow,dispatchability)", true)]
    [InlineData("trip.assign_driver", "person-1", false)]
    [InlineData("trip.status", "in_progress (override:workflow)", false)]
    public void IsOverrideAuditEntry_detects_assignment_overrides(string action, string result, bool expected)
    {
        Assert.Equal(expected, DispatchOverrideReportRules.IsOverrideAuditEntry(action, result));
    }

    [Fact]
    public void ParseOverrideKinds_splits_override_tokens()
    {
        var kinds = DispatchOverrideReportRules.ParseOverrideKinds(
            "driver-1 (override:availability,workflow)");

        Assert.Equal(["availability", "workflow"], kinds);
    }
}

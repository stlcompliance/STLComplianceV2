using StaffArr.Api.Contracts;
using StaffArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PersonnelHistoryRulesTests
{
    [Fact]
    public void IsStale_returns_true_when_never_computed()
    {
        var asOf = DateTimeOffset.UtcNow;
        Assert.True(PersonnelHistoryRules.IsStale(null, asOf, 1));
    }

    [Fact]
    public void AggregateCategoryCounts_groups_timeline_categories()
    {
        var now = DateTimeOffset.UtcNow;
        var personId = Guid.NewGuid();
        var entries = new List<PersonTimelineEntryResponse>
        {
            new("a", personId, "incident", "incident_reported", "t", null, now, null, "x", "1", null),
            new("b", personId, "certification", "certification_granted", "t", null, now, null, "x", "2", null),
            new("c", personId, "permission", "assignment_created", "t", null, now, null, "x", "3", null),
        };

        var counts = PersonnelHistoryRules.AggregateCategoryCounts(entries);
        Assert.Equal(1, counts.IncidentCount);
        Assert.Equal(1, counts.CertificationCount);
        Assert.Equal(1, counts.PermissionCount);
    }
}

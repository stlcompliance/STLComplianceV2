using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using Xunit;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchExceptionRulesTests
{
    [Fact]
    public void ComputeDefaultSlaDueAt_uses_category_hours()
    {
        var created = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var due = DispatchExceptionRules.ComputeDefaultSlaDueAt(
            DispatchExceptionCategories.Delay,
            created);
        Assert.Equal(created.AddHours(2), due);
    }

    [Fact]
    public void BuildResolutionNotes_includes_template_label()
    {
        var notes = DispatchExceptionRules.BuildResolutionNotes(
            DispatchExceptionResolutionTemplates.ReassignDriver,
            "Driver swapped at yard.");
        Assert.Contains("Reassign driver", notes);
        Assert.Contains("Driver swapped at yard.", notes);
    }

    [Fact]
    public void IsSlaBreached_when_open_queue_and_past_due()
    {
        var entity = new DispatchException
        {
            Status = DispatchExceptionStatuses.Open,
            SlaDueAt = DateTimeOffset.UtcNow.AddHours(-1),
        };
        Assert.True(DispatchExceptionRules.IsSlaBreached(entity, DateTimeOffset.UtcNow));
    }
}

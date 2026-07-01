using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using Xunit;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class ProcurementExceptionRulesTests
{
    [Fact]
    public void ComputeDefaultSlaDueAt_uses_category_hours()
    {
        var created = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var due = ProcurementExceptionRules.ComputeDefaultSlaDueAt(
            ProcurementExceptionCategories.BudgetOverride,
            created);
        Assert.Equal(created.AddHours(24), due);
    }

    [Fact]
    public void BuildResolutionNotes_includes_template_label()
    {
        var notes = ProcurementExceptionRules.BuildResolutionNotes(
            ProcurementExceptionResolutionTemplates.SupplierRequote,
            "Supplier sent revised quote.");
        Assert.Contains("Supplier re-quote", notes);
        Assert.Contains("Supplier sent revised quote.", notes);
    }

    [Fact]
    public void IsSlaBreached_when_active_and_past_due()
    {
        var entity = new ProcurementException
        {
            Status = ProcurementExceptionStatuses.Open,
            SlaDueAt = DateTimeOffset.UtcNow.AddHours(-1),
        };
        Assert.True(ProcurementExceptionRules.IsSlaBreached(entity, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void CanTransition_allows_cancelled_to_investigating_for_reopen()
    {
        Assert.True(ProcurementExceptionRules.CanTransition(
            ProcurementExceptionStatuses.Cancelled,
            ProcurementExceptionStatuses.Investigating));
    }
}

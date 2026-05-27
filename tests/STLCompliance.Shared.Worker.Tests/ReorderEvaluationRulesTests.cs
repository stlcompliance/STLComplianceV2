using SupplyArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class ReorderEvaluationRulesTests
{
    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(11, 10, false)]
    public void NeedsReorder_compares_available_quantity_to_reorder_point(
        decimal availableQuantity,
        decimal reorderPoint,
        bool expected) =>
        Assert.Equal(expected, ReorderEvaluationRules.NeedsReorder(availableQuantity, reorderPoint));

    [Theory]
    [InlineData(2, 10, 25.0, 25.0)]
    [InlineData(2, 10, -1.0, 8.0)]
    [InlineData(10, 10, -1.0, 10.0)]
    public void ResolveSuggestedQuantity_uses_reorder_quantity_or_deficit(
        decimal availableQuantity,
        decimal reorderPoint,
        double reorderQuantityOrSentinel,
        decimal expected)
    {
        decimal? reorderQuantity = reorderQuantityOrSentinel < 0 ? null : (decimal)reorderQuantityOrSentinel;
        Assert.Equal(
            expected,
            ReorderEvaluationRules.ResolveSuggestedQuantity(availableQuantity, reorderPoint, reorderQuantity));
    }

    [Theory]
    [InlineData("draft", true)]
    [InlineData("submitted", true)]
    [InlineData("approved", false)]
    public void IsOpenPurchaseRequestStatus_detects_open_workflow_states(string status, bool expected) =>
        Assert.Equal(expected, ReorderEvaluationRules.IsOpenPurchaseRequestStatus(status));
}

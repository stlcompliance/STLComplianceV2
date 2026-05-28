using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class RecertificationAssignmentRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(30, 30)]
    [InlineData(500, 365)]
    [InlineData(null, 30)]
    public void NormalizeLeadDays_clamps(int? input, int expected) =>
        Assert.Equal(expected, RecertificationAssignmentRules.NormalizeLeadDays(input));

    [Theory]
    [InlineData("issued", true)]
    [InlineData("suspended", true)]
    [InlineData("expired", false)]
    [InlineData("revoked", false)]
    public void ShouldAssign_respects_status(string status, bool expected)
    {
        var expiresAt = AsOf.AddDays(10);
        Assert.Equal(
            expected,
            RecertificationAssignmentRules.ShouldAssign(status, expiresAt, null, AsOf, 30));
    }

    [Fact]
    public void ShouldAssign_requires_future_expiry_within_lead_window()
    {
        Assert.True(RecertificationAssignmentRules.ShouldAssign(
            "issued",
            AsOf.AddDays(15),
            null,
            AsOf,
            30));

        Assert.False(RecertificationAssignmentRules.ShouldAssign(
            "issued",
            AsOf.AddDays(-1),
            null,
            AsOf,
            30));

        Assert.False(RecertificationAssignmentRules.ShouldAssign(
            "issued",
            AsOf.AddDays(45),
            null,
            AsOf,
            30));
    }

    [Fact]
    public void ShouldAssign_uses_grant_publication_expiry_fallback()
    {
        Assert.True(RecertificationAssignmentRules.ShouldAssign(
            "issued",
            null,
            AsOf.AddDays(7),
            AsOf,
            30));
    }
}

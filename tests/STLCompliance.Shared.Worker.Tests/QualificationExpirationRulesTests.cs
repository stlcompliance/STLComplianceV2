using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class QualificationExpirationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("issued", true)]
    [InlineData("suspended", true)]
    [InlineData("revoked", false)]
    [InlineData("expired", false)]
    public void IsExpirableStatus_matches_active_lifecycle_states(string status, bool expected) =>
        Assert.Equal(expected, QualificationExpirationRules.IsExpirableStatus(status));

    [Fact]
    public void ResolveEffectiveExpiresAt_prefers_issue_expiry_over_grant() =>
        Assert.Equal(
            AsOf.AddDays(-1),
            QualificationExpirationRules.ResolveEffectiveExpiresAt(
                AsOf.AddDays(-1),
                AsOf.AddDays(30)));

    [Fact]
    public void ResolveEffectiveExpiresAt_falls_back_to_grant_publication_expiry() =>
        Assert.Equal(
            AsOf.AddDays(7),
            QualificationExpirationRules.ResolveEffectiveExpiresAt(null, AsOf.AddDays(7)));

    [Theory]
    [InlineData("issued", -1, true)]
    [InlineData("issued", 1, false)]
    [InlineData("issued", 0, true)]
    [InlineData("suspended", -1, true)]
    [InlineData("revoked", -1, false)]
    [InlineData("issued", -1, true, true)]
    public void ShouldExpire_uses_effective_expiry_and_status(
        string status,
        int expiryOffsetDays,
        bool expected,
        bool useGrantFallback = false)
    {
        var issueExpires = useGrantFallback ? (DateTimeOffset?)null : AsOf.AddDays(expiryOffsetDays);
        var grantExpires = useGrantFallback ? AsOf.AddDays(expiryOffsetDays) : (DateTimeOffset?)null;

        Assert.Equal(
            expected,
            QualificationExpirationRules.ShouldExpire(status, issueExpires, grantExpires, AsOf));
    }

    [Fact]
    public void ShouldExpire_returns_false_when_no_expiry_is_configured() =>
        Assert.False(QualificationExpirationRules.ShouldExpire("issued", null, null, AsOf));
}

using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class ServiceTokenCleanupRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(-10, 7, true)]
    [InlineData(-3, 7, false)]
    [InlineData(-7, 7, true)]
    public void IsExpiredPurgeCandidate_respects_grace_period(
        int expiryOffsetDays,
        int graceDays,
        bool expected)
    {
        var expiresAt = AsOf.AddDays(expiryOffsetDays);
        Assert.Equal(
            expected,
            ServiceTokenCleanupRules.IsExpiredPurgeCandidate(null, expiresAt, AsOf, graceDays));
    }

    [Fact]
    public void IsExpiredPurgeCandidate_ignores_revoked_tokens() =>
        Assert.False(
            ServiceTokenCleanupRules.IsExpiredPurgeCandidate(
                AsOf.AddDays(-40),
                AsOf.AddDays(-40),
                AsOf,
                7));

    [Theory]
    [InlineData(-40, 30, true)]
    [InlineData(-10, 30, false)]
    public void IsRevokedPurgeCandidate_respects_grace_period(
        int revokedOffsetDays,
        int graceDays,
        bool expected)
    {
        var revokedAt = AsOf.AddDays(revokedOffsetDays);
        Assert.Equal(
            expected,
            ServiceTokenCleanupRules.IsRevokedPurgeCandidate(revokedAt, AsOf, graceDays));
    }

    [Fact]
    public void ResolveCleanupReason_prefers_revoked_over_expired()
    {
        var reason = ServiceTokenCleanupRules.ResolveCleanupReason(
            AsOf.AddDays(-40),
            AsOf.AddDays(-40),
            AsOf,
            7,
            30);
        Assert.Equal("revoked", reason);
    }
}

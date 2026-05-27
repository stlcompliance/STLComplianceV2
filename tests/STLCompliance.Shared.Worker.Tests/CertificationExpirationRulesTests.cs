using StaffArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class CertificationExpirationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("active", true)]
    [InlineData("expired", false)]
    [InlineData("revoked", false)]
    public void IsExpirableStatus_matches_active_records_only(string status, bool expected) =>
        Assert.Equal(expected, CertificationExpirationRules.IsExpirableStatus(status));

    [Theory]
    [InlineData("active", -1, true)]
    [InlineData("active", 1, false)]
    [InlineData("active", 0, true)]
    [InlineData("revoked", -1, false)]
    [InlineData("expired", -1, false)]
    public void ShouldExpire_uses_expiry_and_status(
        string status,
        int expiryOffsetDays,
        bool expected)
    {
        var expiresAt = AsOf.AddDays(expiryOffsetDays);
        Assert.Equal(
            expected,
            CertificationExpirationRules.ShouldExpire(status, expiresAt, AsOf));
    }

    [Fact]
    public void ShouldExpire_returns_false_when_no_expiry_is_configured() =>
        Assert.False(CertificationExpirationRules.ShouldExpire("active", null, AsOf));
}

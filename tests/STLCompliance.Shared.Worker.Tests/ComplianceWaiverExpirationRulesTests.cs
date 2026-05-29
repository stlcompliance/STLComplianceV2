using ComplianceCore.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class ComplianceWaiverExpirationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("approved", true)]
    [InlineData("pending", false)]
    [InlineData("rejected", false)]
    [InlineData("revoked", false)]
    [InlineData("expired", false)]
    public void IsExpirableStatus_matches_approved_records_only(string status, bool expected) =>
        Assert.Equal(expected, ComplianceWaiverRules.IsExpirableStatus(status));

    [Theory]
    [InlineData("approved", -1, true)]
    [InlineData("approved", 1, false)]
    [InlineData("approved", 0, true)]
    [InlineData("revoked", -1, false)]
    [InlineData("expired", -1, false)]
    public void ShouldExpireForBatch_uses_expiry_and_status(
        string status,
        int expiryOffsetDays,
        bool expected)
    {
        var expiresAt = AsOf.AddDays(expiryOffsetDays);
        Assert.Equal(
            expected,
            ComplianceWaiverRules.ShouldExpireForBatch(status, expiresAt, AsOf));
    }

    [Fact]
    public void ShouldExpireForBatch_returns_false_when_no_expiry_is_configured() =>
        Assert.False(ComplianceWaiverRules.ShouldExpireForBatch("approved", null, AsOf));

    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, ComplianceWaiverRules.NormalizeBatchSize(input));
}

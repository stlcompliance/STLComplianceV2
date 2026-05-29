using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class PlatformOutboxRulesTests
{
    [Fact]
    public void BuildIdempotencyKey_is_stable()
    {
        var key = PlatformOutboxRules.BuildIdempotencyKey(
            "tenant.updated",
            "tenant",
            "abc",
            "123");
        Assert.Equal("tenant.updated:tenant:abc:123", key);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    public void IsReadyForProcessing_respects_next_retry(int? minutesFromNow, bool expectedReady)
    {
        var asOf = DateTimeOffset.UtcNow;
        DateTimeOffset? nextRetry = minutesFromNow switch
        {
            null => null,
            -1 => asOf.AddMinutes(-1),
            1 => asOf.AddMinutes(1),
            _ => asOf,
        };

        Assert.Equal(expectedReady, PlatformOutboxRules.IsReadyForProcessing(nextRetry, asOf));
    }
}

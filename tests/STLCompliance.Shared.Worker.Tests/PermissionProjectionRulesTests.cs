using StaffArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PermissionProjectionRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, 1, true)]
    [InlineData(-2, 1, true)]
    [InlineData(-1, 1, false)]
    [InlineData(-3, 2, true)]
    public void IsStale_uses_computed_at_and_staleness_window(
        int? computedOffsetHours,
        int stalenessHours,
        bool expected)
    {
        DateTimeOffset? computedAt = computedOffsetHours is int offset
            ? AsOf.AddHours(offset)
            : null;

        Assert.Equal(expected, PermissionProjectionRules.IsStale(computedAt, AsOf, stalenessHours));
    }

    [Theory]
    [InlineData("staffarr.people.read", "tenant", null, "staffarr.people.read|tenant|")]
    [InlineData("staffarr.incidents.manage", "site", "site-1", "staffarr.incidents.manage|site|site-1")]
    public void BuildPermissionIdentity_includes_key_scope_type_and_value(
        string permissionKey,
        string scopeType,
        string? scopeValue,
        string expected) =>
        Assert.Equal(expected, PermissionProjectionRules.BuildPermissionIdentity(permissionKey, scopeType, scopeValue));

    [Theory]
    [InlineData(null, 100)]
    [InlineData(600, 500)]
    [InlineData(0, 1)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, PermissionProjectionRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 1)]
    [InlineData(200, 168)]
    [InlineData(0, 1)]
    public void NormalizeStalenessHours_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, PermissionProjectionRules.NormalizeStalenessHours(input));
}

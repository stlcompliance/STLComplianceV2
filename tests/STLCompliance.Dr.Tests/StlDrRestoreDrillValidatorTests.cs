using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

[Trait("Category", "Dr")]
public sealed class StlDrRestoreDrillValidatorTests
{
    [Fact]
    public async Task ValidateRestoredDatabaseAsync_fails_for_invalid_connection()
    {
        var connectionString = StlDrRestoreDrillSupport.BuildAdminConnectionString(
            "127.0.0.1",
            1,
            "invalid",
            "invalid",
            "missing_database");

        var result = await StlDrRestoreDrillValidator.ValidateRestoredDatabaseAsync(connectionString);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}

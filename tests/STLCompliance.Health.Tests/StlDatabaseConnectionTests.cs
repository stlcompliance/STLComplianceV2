using Npgsql;
using STLCompliance.Shared.Data;

namespace STLCompliance.Health.Tests;

public class StlDatabaseConnectionTests
{
    [Theory]
    [InlineData("Host=localhost;Port=5432;Database=nexarr;Username=stl;Password=secret")]
    [InlineData("Server=db.internal;Database=app;User Id=user;Password=pass")]
    public void Normalize_PreservesAdoNetConnectionStrings(string connectionString)
    {
        Assert.Equal(connectionString, StlDatabaseConnection.Normalize(connectionString));
    }

    [Theory]
    [InlineData("postgresql://nexarr:secret@dpg-example-a/nexarr")]
    [InlineData("postgres://nexarr:secret@dpg-example-a:5432/nexarr")]
    [InlineData("postgresql://nexarr:p%40ss%23word@dpg-example-a/nexarr?sslmode=require")]
    public void Normalize_ConvertsPostgresUriToNpgsqlFormat(string uri)
    {
        var normalized = StlDatabaseConnection.Normalize(uri);
        Assert.NotNull(normalized);

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        Assert.Equal("dpg-example-a", builder.Host);
        Assert.Equal("nexarr", builder.Database);
        Assert.Equal("nexarr", builder.Username);
        Assert.False(string.IsNullOrEmpty(builder.Password));
    }

    [Fact]
    public void Normalize_ReturnsNullForBlankValues()
    {
        Assert.Null(StlDatabaseConnection.Normalize(null));
        Assert.Null(StlDatabaseConnection.Normalize("   "));
    }
}

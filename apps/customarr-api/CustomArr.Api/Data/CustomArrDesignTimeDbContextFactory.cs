using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace CustomArr.Api.Data;

public sealed class CustomArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomArrDbContext>
{
    public CustomArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=customarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<CustomArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CustomArrDbContext(options);
    }
}

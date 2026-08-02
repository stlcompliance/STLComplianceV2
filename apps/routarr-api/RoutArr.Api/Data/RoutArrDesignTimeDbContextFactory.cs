using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoutArr.Api.Data;
using STLCompliance.Shared.Data;

namespace RoutArr.Api.Data;

public sealed class RoutArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RoutArrDbContext>
{
    public RoutArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=routarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<RoutArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new RoutArrDbContext(options);
    }
}

using LoadArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace LoadArr.Api.Data;

public sealed class LoadArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LoadArrDbContext>
{
    public LoadArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=loadarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<LoadArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LoadArrDbContext(options);
    }
}

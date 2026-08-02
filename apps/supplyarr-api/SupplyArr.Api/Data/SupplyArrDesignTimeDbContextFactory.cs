using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;
using SupplyArr.Api.Data;

namespace SupplyArr.Api.Data;

public sealed class SupplyArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SupplyArrDbContext>
{
    public SupplyArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=supplyarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<SupplyArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SupplyArrDbContext(options);
    }
}

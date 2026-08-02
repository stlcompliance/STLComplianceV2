using LedgArr.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace LedgArr.Api.Data;

public sealed class LedgArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LedgArrDbContext>
{
    public LedgArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=ledgarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<LedgArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LedgArrDbContext(options);
    }
}

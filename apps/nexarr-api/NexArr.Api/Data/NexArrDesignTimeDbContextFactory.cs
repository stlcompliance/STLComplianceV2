using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NexArr.Api.Data;
using STLCompliance.Shared.Data;

namespace NexArr.Api.Data;

public sealed class NexArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<NexArrDbContext>
{
    public NexArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=nexarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<NexArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NexArrDbContext(options);
    }
}

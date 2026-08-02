using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;
using TrainArr.Api.Data;

namespace TrainArr.Api.Data;

public sealed class TrainArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TrainArrDbContext>
{
    public TrainArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=trainarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<TrainArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TrainArrDbContext(options);
    }
}

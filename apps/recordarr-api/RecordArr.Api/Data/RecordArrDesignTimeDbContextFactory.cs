using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace RecordArr.Api.Data;

public sealed class RecordArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RecordArrDbContext>
{
    public RecordArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=recordarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new RecordArrDbContext(options);
    }
}

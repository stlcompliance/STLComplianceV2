using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Data;

public sealed class MaintainArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MaintainArrDbContext>
{
    public MaintainArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=maintainarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MaintainArrDbContext(options);
    }
}

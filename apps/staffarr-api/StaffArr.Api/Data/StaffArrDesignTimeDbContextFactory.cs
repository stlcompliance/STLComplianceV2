using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StaffArr.Api.Data;
using STLCompliance.Shared.Data;

namespace StaffArr.Api.Data;

public sealed class StaffArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<StaffArrDbContext>
{
    public StaffArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=staffarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<StaffArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new StaffArrDbContext(options);
    }
}

using AssurArr.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace AssurArr.Api.Data;

public sealed class AssurArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AssurArrDbContext>
{
    public AssurArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=assurarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AssurArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AssurArrDbContext(options, new HttpContextAccessor());
    }
}

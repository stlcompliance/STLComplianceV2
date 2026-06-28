using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrdArr.Api.Data;

public sealed class OrdArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrdArrDbContext>
{
    public OrdArrDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Database=ordarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<OrdArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OrdArrDbContext(options);
    }
}

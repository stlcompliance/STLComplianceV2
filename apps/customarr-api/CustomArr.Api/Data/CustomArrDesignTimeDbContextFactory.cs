using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CustomArr.Api.Data;

public sealed class CustomArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomArrDbContext>
{
    public CustomArrDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Database=customarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<CustomArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CustomArrDbContext(options);
    }
}

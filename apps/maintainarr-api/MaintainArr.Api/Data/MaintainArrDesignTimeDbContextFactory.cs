using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MaintainArr.Api.Data;

public sealed class MaintainArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MaintainArrDbContext>
{
    public MaintainArrDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Database=maintainarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MaintainArrDbContext(options);
    }
}

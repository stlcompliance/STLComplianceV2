using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RecordArr.Api.Data;

public sealed class RecordArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RecordArrDbContext>
{
    public RecordArrDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Database=recordarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new RecordArrDbContext(options);
    }
}

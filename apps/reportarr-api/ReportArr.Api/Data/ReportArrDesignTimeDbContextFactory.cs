using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ReportArr.Api.Data;
using STLCompliance.Shared.Data;

namespace ReportArr.Api.Data;

public sealed class ReportArrDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReportArrDbContext>
{
    public ReportArrDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=reportarr_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ReportArrDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ReportArrDbContext(options);
    }
}

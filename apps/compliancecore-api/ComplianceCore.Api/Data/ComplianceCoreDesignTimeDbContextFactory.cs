using ComplianceCore.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Data;

public sealed class ComplianceCoreDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ComplianceCoreDbContext>
{
    public ComplianceCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = StlDatabaseConnection.ResolveFromEnvironment()
            ?? "Host=localhost;Database=compliancecore_design;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ComplianceCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ComplianceCoreDbContext(options);
    }
}

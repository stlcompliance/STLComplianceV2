using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace ReportArr.Api.Data;

public sealed class ReportArrDbContext(DbContextOptions<ReportArrDbContext> options) : PlatformDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace LoadArr.Api.Data;

public sealed class LoadArrDbContext(DbContextOptions<LoadArrDbContext> options) : PlatformDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace RecordArr.Api.Data;

public sealed class RecordArrDbContext(DbContextOptions<RecordArrDbContext> options) : PlatformDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}

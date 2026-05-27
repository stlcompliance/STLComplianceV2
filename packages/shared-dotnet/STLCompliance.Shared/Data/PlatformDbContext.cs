using Microsoft.EntityFrameworkCore;

namespace STLCompliance.Shared.Data;

public abstract class PlatformDbContext : DbContext
{
    protected PlatformDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<PlatformMetadata> PlatformMetadata => Set<PlatformMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlatformMetadata>(entity =>
        {
            entity.ToTable("platform_metadata");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(128);
            entity.Property(x => x.ModifiedBy).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            entity.HasIndex(x => x.TenantId);
        });
    }
}

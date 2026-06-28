using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace OrdArr.Api.Data;

public sealed class OrdArrDbContext(DbContextOptions<OrdArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<OrdArrOrderRecord> OrderRecords => Set<OrdArrOrderRecord>();
    public DbSet<OrdArrIdempotencyRecord> IdempotencyRecords => Set<OrdArrIdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrdArrOrderRecord>(entity =>
        {
            entity.ToTable("ordarr_order_records");
            entity.HasKey(x => x.OrderId);
            entity.Property(x => x.OrderId).HasMaxLength(64);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LifecycleStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CustomerDisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.LifecycleStatus, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.UpdatedAt });
        });

        modelBuilder.Entity<OrdArrIdempotencyRecord>(entity =>
        {
            entity.ToTable("ordarr_idempotency_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.TenantId).IsRequired();
            entity.Property(x => x.OperationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ResourceId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OperationKey, x.IdempotencyKey }).IsUnique();
        });
    }
}

public sealed class OrdArrOrderRecord
{
    public string OrderId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string LifecycleStatus { get; set; } = "draft";
    public string CustomerDisplayName { get; set; } = string.Empty;
    public string OwnerPersonId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class OrdArrIdempotencyRecord
{
    public string Id { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string OperationKey { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

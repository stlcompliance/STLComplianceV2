using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.SmartImport;

namespace STLCompliance.Shared.Data;

public abstract class PlatformDbContext : DbContext
{
    protected PlatformDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<PlatformMetadata> PlatformMetadata => Set<PlatformMetadata>();

    public DbSet<SmartImportDestinationRecord> SmartImportDestinationRecords =>
        Set<SmartImportDestinationRecord>();

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

        modelBuilder.Entity<SmartImportDestinationRecord>(entity =>
        {
            entity.ToTable("smart_import_destination_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DestinationProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Operation).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.RecordArrSourceRecordId).HasMaxLength(128);
            entity.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DestinationProduct, x.IdempotencyKey })
                .IsUnique()
                .HasDatabaseName("IX_smart_import_destination_records_idempotency");
            entity.HasIndex(x => new { x.TenantId, x.DestinationProduct, x.EntityType, x.CreatedAt })
                .HasDatabaseName("IX_smart_import_destination_records_product_entity_created");
        });
    }
}

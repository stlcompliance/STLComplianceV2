using Microsoft.EntityFrameworkCore;
using LoadArr.Api.Settings;
using STLCompliance.Shared.Data;

namespace LoadArr.Api.Data;

public sealed class LoadArrDbContext(DbContextOptions<LoadArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<LoadArrReceivingSessionEntity> LoadArrReceivingSessions => Set<LoadArrReceivingSessionEntity>();

    public DbSet<LoadArrInventoryOriginEventEntity> LoadArrInventoryOriginEvents =>
        Set<LoadArrInventoryOriginEventEntity>();

    public DbSet<LoadArrInventoryMovementEntity> LoadArrInventoryMovements =>
        Set<LoadArrInventoryMovementEntity>();

    public DbSet<LoadArrInventoryBalanceEntity> LoadArrInventoryBalances =>
        Set<LoadArrInventoryBalanceEntity>();

    public DbSet<LoadArrWarehouseTaskEntity> LoadArrWarehouseTasks =>
        Set<LoadArrWarehouseTaskEntity>();

    public DbSet<LoadArrTenantSettings> LoadArrTenantSettings => Set<LoadArrTenantSettings>();

    public DbSet<LoadArrTenantSettingAuditEntry> LoadArrTenantSettingAuditEntries =>
        Set<LoadArrTenantSettingAuditEntry>();

    public DbSet<LoadArrTransferOrderEntity> LoadArrTransferOrders => Set<LoadArrTransferOrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoadArrReceivingSessionEntity>(entity =>
        {
            entity.ToTable("loadarr_receiving_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SessionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReceivingNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReceivingType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ClientRequestId).HasMaxLength(128);
            entity.Property(x => x.RequestFingerprint).HasMaxLength(2048);
            entity.Property(x => x.CompletedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SessionId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ClientRequestId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.StartedAtUtc });
        });

        modelBuilder.Entity<LoadArrTenantSettings>(entity =>
        {
            entity.ToTable("loadarr_tenant_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SettingsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.NormalizedSnapshotJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.RowVersion).HasMaxLength(64).IsConcurrencyToken().IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.UpdatedByPersonId).HasMaxLength(128);
            entity.Property(x => x.UpdatedByDisplayNameSnapshot).HasMaxLength(256);
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        modelBuilder.Entity<LoadArrTenantSettingAuditEntry>(entity =>
        {
            entity.ToTable("loadarr_tenant_setting_audit_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SectionKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.ChangedByPersonId).HasMaxLength(128);
            entity.Property(x => x.ChangedByDisplayNameSnapshot).HasMaxLength(256);
            entity.Property(x => x.Reason).HasMaxLength(1024);
            entity.Property(x => x.ChangeSource).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BeforeSummaryJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.AfterSummaryJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ChangedFieldsJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.WarningsAcknowledgedJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ChangedAt });
            entity.HasIndex(x => new { x.TenantId, x.SectionKey, x.ChangedAt });
        });

        modelBuilder.Entity<LoadArrTransferOrderEntity>(entity =>
        {
            entity.ToTable("loadarr_transfer_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TransferNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TransferType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ClientRequestId).HasMaxLength(128);
            entity.Property(x => x.RequestFingerprint).HasMaxLength(2048);
            entity.Property(x => x.CompletedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OrderId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ClientRequestId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAtUtc });
        });

        modelBuilder.Entity<LoadArrInventoryOriginEventEntity>(entity =>
        {
            entity.ToTable("loadarr_inventory_origin_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OriginEventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginObjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.WarehouseLocationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SupplyarrItemId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.OriginEventId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.OriginObjectType, x.OriginObjectId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<LoadArrInventoryMovementEntity>(entity =>
        {
            entity.ToTable("loadarr_inventory_movements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MovementId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MovementType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FromLocationId).HasMaxLength(64);
            entity.Property(x => x.ToLocationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SupplyarrItemId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RelatedObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedObjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.InventoryOriginEventId).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.MovementId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RelatedObjectType, x.RelatedObjectId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<LoadArrInventoryBalanceEntity>(entity =>
        {
            entity.ToTable("loadarr_inventory_balances");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BalanceId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SupplyarrItemId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LocationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LotCode).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SerialCode).HasMaxLength(128).IsRequired();
            entity.Property(x => x.State).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.BalanceId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrItemId, x.LocationId, x.LotCode, x.SerialCode }).IsUnique();
        });

        modelBuilder.Entity<LoadArrWarehouseTaskEntity>(entity =>
        {
            entity.ToTable("loadarr_warehouse_tasks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TaskId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TaskType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SupplyarrItemId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.TaskId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceObjectType, x.SourceObjectId, x.TaskType });
        });
    }
}

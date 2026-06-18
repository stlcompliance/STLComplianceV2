using Microsoft.EntityFrameworkCore;
using LoadArr.Api.Settings;
using STLCompliance.Shared.Data;

namespace LoadArr.Api.Data;

public sealed class LoadArrDbContext(DbContextOptions<LoadArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<LoadArrTenantSettings> LoadArrTenantSettings => Set<LoadArrTenantSettings>();

    public DbSet<LoadArrTenantSettingAuditEntry> LoadArrTenantSettingAuditEntries =>
        Set<LoadArrTenantSettingAuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}

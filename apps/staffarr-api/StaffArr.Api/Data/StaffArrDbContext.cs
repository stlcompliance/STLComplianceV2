using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace StaffArr.Api.Data;

public sealed class StaffArrDbContext(DbContextOptions<StaffArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<StaffPerson> People => Set<StaffPerson>();

    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();

    public DbSet<StaffArrAuditEvent> AuditEvents => Set<StaffArrAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrgUnit>(entity =>
        {
            entity.ToTable("staffarr_org_units");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UnitType, x.Name }).IsUnique();
            entity.HasOne(x => x.ParentOrgUnit).WithMany().HasForeignKey(x => x.ParentOrgUnitId);
        });

        modelBuilder.Entity<StaffPerson>(entity =>
        {
            entity.ToTable("staffarr_people");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GivenName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FamilyName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PrimaryEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.EmploymentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.JobTitle).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PrimaryEmail });
            entity.HasIndex(x => new { x.TenantId, x.ExternalUserId }).IsUnique();
            entity.HasOne(x => x.PrimaryOrgUnit).WithMany().HasForeignKey(x => x.PrimaryOrgUnitId);
            entity.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerPersonId);
        });

        modelBuilder.Entity<StaffArrAuditEvent>(entity =>
        {
            entity.ToTable("staffarr_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.OccurredAt);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace StaffArr.Api.Data;

public sealed class StaffArrDbContext(DbContextOptions<StaffArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<StaffPerson> People => Set<StaffPerson>();

    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();

    public DbSet<OrgUnitAssignment> OrgUnitAssignments => Set<OrgUnitAssignment>();

    public DbSet<RoleTemplate> RoleTemplates => Set<RoleTemplate>();

    public DbSet<PermissionTemplate> PermissionTemplates => Set<PermissionTemplate>();

    public DbSet<RoleTemplatePermission> RoleTemplatePermissions => Set<RoleTemplatePermission>();

    public DbSet<PersonRoleAssignment> PersonRoleAssignments => Set<PersonRoleAssignment>();

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
            entity.HasIndex(x => new { x.TenantId, x.ManagerPersonId });
            entity.HasOne(x => x.PrimaryOrgUnit).WithMany().HasForeignKey(x => x.PrimaryOrgUnitId);
            entity.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerPersonId);
        });

        modelBuilder.Entity<OrgUnitAssignment>(entity =>
        {
            entity.ToTable("staffarr_org_unit_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.PersonId,
                x.SiteOrgUnitId,
                x.DepartmentOrgUnitId,
                x.TeamOrgUnitId,
                x.PositionOrgUnitId
            }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.SiteOrgUnitId);
            entity.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.DepartmentOrgUnitId);
            entity.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.TeamOrgUnitId);
            entity.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.PositionOrgUnitId);
        });

        modelBuilder.Entity<RoleTemplate>(entity =>
        {
            entity.ToTable("staffarr_role_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoleKey }).IsUnique();
        });

        modelBuilder.Entity<PermissionTemplate>(entity =>
        {
            entity.ToTable("staffarr_permission_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PermissionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PermissionKey }).IsUnique();
        });

        modelBuilder.Entity<RoleTemplatePermission>(entity =>
        {
            entity.ToTable("staffarr_role_template_permissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeValue).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.RoleTemplateId,
                x.PermissionTemplateId,
                x.ScopeType,
                x.ScopeValue
            }).IsUnique();
            entity.HasOne<RoleTemplate>().WithMany().HasForeignKey(x => x.RoleTemplateId);
            entity.HasOne<PermissionTemplate>().WithMany().HasForeignKey(x => x.PermissionTemplateId);
        });

        modelBuilder.Entity<PersonRoleAssignment>(entity =>
        {
            entity.ToTable("staffarr_person_role_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeValue).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.PersonId,
                x.RoleTemplateId,
                x.ScopeType,
                x.ScopeValue
            }).IsUnique();
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<RoleTemplate>().WithMany().HasForeignKey(x => x.RoleTemplateId);
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

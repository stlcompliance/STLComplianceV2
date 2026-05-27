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

    public DbSet<PermissionHistoryEvent> PermissionHistoryEvents => Set<PermissionHistoryEvent>();

    public DbSet<CertificationDefinition> CertificationDefinitions => Set<CertificationDefinition>();

    public DbSet<PersonCertification> PersonCertifications => Set<PersonCertification>();

    public DbSet<PersonReadinessOverride> PersonReadinessOverrides => Set<PersonReadinessOverride>();

    public DbSet<PersonnelIncident> PersonnelIncidents => Set<PersonnelIncident>();

    public DbSet<PersonTrainingBlocker> PersonTrainingBlockers => Set<PersonTrainingBlocker>();

    public DbSet<IncidentTrainarrRouting> IncidentTrainarrRoutings => Set<IncidentTrainarrRouting>();

    public DbSet<ReadinessRollup> ReadinessRollups => Set<ReadinessRollup>();

    public DbSet<PersonPermissionProjection> PersonPermissionProjections => Set<PersonPermissionProjection>();

    public DbSet<PersonPermissionProjectionEntry> PersonPermissionProjectionEntries => Set<PersonPermissionProjectionEntry>();

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

        modelBuilder.Entity<PermissionHistoryEvent>(entity =>
        {
            entity.ToTable("staffarr_permission_history_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssignmentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RoleKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RoleName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PermissionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PermissionName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeValue).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.AssignmentId, x.OccurredAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<PersonRoleAssignment>().WithMany().HasForeignKey(x => x.AssignmentId);
            entity.HasOne<RoleTemplate>().WithMany().HasForeignKey(x => x.RoleTemplateId);
            entity.HasOne<PermissionTemplate>().WithMany().HasForeignKey(x => x.PermissionTemplateId);
        });

        modelBuilder.Entity<CertificationDefinition>(entity =>
        {
            entity.ToTable("staffarr_certification_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CertificationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Category).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CertificationKey }).IsUnique();
        });

        modelBuilder.Entity<PersonCertification>(entity =>
        {
            entity.ToTable("staffarr_person_certifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.CertificationDefinitionId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.ExternalPublicationId }).IsUnique()
                .HasFilter("\"ExternalPublicationId\" IS NOT NULL");
            entity.HasIndex(x => new { x.TenantId, x.LastExternalLifecyclePublicationId }).IsUnique()
                .HasFilter("\"LastExternalLifecyclePublicationId\" IS NOT NULL");
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasOne<CertificationDefinition>().WithMany().HasForeignKey(x => x.CertificationDefinitionId);
        });

        modelBuilder.Entity<PersonReadinessOverride>(entity =>
        {
            entity.ToTable("staffarr_person_readiness_overrides");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(1024).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.GrantedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<PersonTrainingBlocker>(entity =>
        {
            entity.ToTable("staffarr_person_training_blockers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.QualificationKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.QualificationName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.BlockerType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TrainarrPublicationId }).IsUnique();
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<PersonnelIncident>(entity =>
        {
            entity.ToTable("staffarr_personnel_incidents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReasonCategoryKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4096).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.ReportedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReportedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<IncidentTrainarrRouting>(entity =>
        {
            entity.ToTable("staffarr_incident_trainarr_routings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoutingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.TrainarrRemediationId }).IsUnique();
            entity.HasOne<PersonnelIncident>().WithMany().HasForeignKey(x => x.IncidentId);
        });

        modelBuilder.Entity<ReadinessRollup>(entity =>
        {
            entity.ToTable("staffarr_readiness_rollups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(16).IsRequired();
            entity.Property(x => x.OrgUnitName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReadyPercent).HasPrecision(5, 1);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeType, x.OrgUnitId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ScopeType, x.ComputedAt });
            entity.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId);
        });

        modelBuilder.Entity<PersonPermissionProjection>(entity =>
        {
            entity.ToTable("staffarr_person_permission_projections");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasMany(x => x.Entries).WithOne(x => x.Projection).HasForeignKey(x => x.ProjectionId);
        });

        modelBuilder.Entity<PersonPermissionProjectionEntry>(entity =>
        {
            entity.ToTable("staffarr_person_permission_projection_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PermissionKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PermissionName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeValue).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.PersonId,
                x.PermissionKey,
                x.ScopeType,
                x.ScopeValue
            }).IsUnique();
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
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

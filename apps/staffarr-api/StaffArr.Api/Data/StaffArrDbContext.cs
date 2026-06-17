using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace StaffArr.Api.Data;

public sealed class StaffArrDbContext(DbContextOptions<StaffArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<StaffPerson> People => Set<StaffPerson>();

    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();

    public DbSet<InternalLocation> InternalLocations => Set<InternalLocation>();

    public DbSet<OrgUnitAssignment> OrgUnitAssignments => Set<OrgUnitAssignment>();

    public DbSet<RoleTemplate> RoleTemplates => Set<RoleTemplate>();

    public DbSet<PermissionTemplate> PermissionTemplates => Set<PermissionTemplate>();

    public DbSet<RoleTemplatePermission> RoleTemplatePermissions => Set<RoleTemplatePermission>();

    public DbSet<PersonRoleAssignment> PersonRoleAssignments => Set<PersonRoleAssignment>();

    public DbSet<PermissionHistoryEvent> PermissionHistoryEvents => Set<PermissionHistoryEvent>();

    public DbSet<StaffRole> StaffRoles => Set<StaffRole>();

    public DbSet<StaffRolePermission> StaffRolePermissions => Set<StaffRolePermission>();

    public DbSet<StaffRoleScope> StaffRoleScopes => Set<StaffRoleScope>();

    public DbSet<StaffPersonRole> StaffPersonRoles => Set<StaffPersonRole>();

    public DbSet<PermissionCatalogCacheEntry> PermissionCatalogCacheEntries => Set<PermissionCatalogCacheEntry>();

    public DbSet<PermissionAuditLogEntry> PermissionAuditLogEntries => Set<PermissionAuditLogEntry>();

    public DbSet<CertificationDefinition> CertificationDefinitions => Set<CertificationDefinition>();

    public DbSet<PersonCertification> PersonCertifications => Set<PersonCertification>();

    public DbSet<PersonReadinessOverride> PersonReadinessOverrides => Set<PersonReadinessOverride>();

    public DbSet<PersonnelIncident> PersonnelIncidents => Set<PersonnelIncident>();

    public DbSet<IncidentNote> IncidentNotes => Set<IncidentNote>();

    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();

    public DbSet<IncidentSupplyDemandLine> IncidentSupplyDemandLines => Set<IncidentSupplyDemandLine>();

    public DbSet<IncidentSupplyDemandStatusEvent> IncidentSupplyDemandStatusEvents => Set<IncidentSupplyDemandStatusEvent>();

    public DbSet<PersonnelNote> PersonnelNotes => Set<PersonnelNote>();

    public DbSet<PersonnelDocument> PersonnelDocuments => Set<PersonnelDocument>();

    public DbSet<PersonTrainingBlocker> PersonTrainingBlockers => Set<PersonTrainingBlocker>();

    public DbSet<PersonTrainingAcknowledgement> PersonTrainingAcknowledgements => Set<PersonTrainingAcknowledgement>();

    public DbSet<PersonOffboardingRecord> PersonOffboardingRecords => Set<PersonOffboardingRecord>();

    public DbSet<PersonOffboardingStep> PersonOffboardingSteps => Set<PersonOffboardingStep>();

    public DbSet<PersonnelUpdateRequest> PersonnelUpdateRequests => Set<PersonnelUpdateRequest>();

    public DbSet<IncidentTrainarrRouting> IncidentTrainarrRoutings => Set<IncidentTrainarrRouting>();

    public DbSet<ReadinessRollup> ReadinessRollups => Set<ReadinessRollup>();

    public DbSet<PersonPermissionProjection> PersonPermissionProjections => Set<PersonPermissionProjection>();

    public DbSet<PersonPermissionProjectionEntry> PersonPermissionProjectionEntries => Set<PersonPermissionProjectionEntry>();

    public DbSet<StaffArrAuditEvent> AuditEvents => Set<StaffArrAuditEvent>();

    public DbSet<TenantPersonExportPreset> TenantPersonExportPresets => Set<TenantPersonExportPreset>();

    public DbSet<TenantPersonExportSchedule> TenantPersonExportSchedules => Set<TenantPersonExportSchedule>();

    public DbSet<PersonExportDeliveryRun> PersonExportDeliveryRuns => Set<PersonExportDeliveryRun>();

    public DbSet<PersonExportDeliveryNotification> PersonExportDeliveryNotifications =>
        Set<PersonExportDeliveryNotification>();

    public DbSet<AuditPackageGenerationJob> AuditPackageGenerationJobs => Set<AuditPackageGenerationJob>();

    public DbSet<PersonnelHistoryRollup> PersonnelHistoryRollups => Set<PersonnelHistoryRollup>();

    public DbSet<PersonnelHistoryEvent> PersonnelHistoryEvents => Set<PersonnelHistoryEvent>();

    public DbSet<TenantStaffArrWorkerSettings> TenantStaffArrWorkerSettings => Set<TenantStaffArrWorkerSettings>();

    public DbSet<StaffArrWorkerRun> StaffArrWorkerRuns => Set<StaffArrWorkerRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrgUnit>(entity =>
        {
            entity.ToTable("staffarr_org_units");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(64);
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SiteType).HasMaxLength(32);
            entity.Property(x => x.Timezone).HasMaxLength(64);
            entity.Property(x => x.Phone).HasMaxLength(32);
            entity.Property(x => x.EmergencyContact).HasMaxLength(256);
            entity.Property(x => x.TeamType).HasMaxLength(32);
            entity.Property(x => x.PositionCode).HasMaxLength(64);
            entity.Property(x => x.ComplianceSensitive).HasDefaultValue(false);
            entity.Property(x => x.SafetySensitive).HasDefaultValue(false);
            entity.Property(x => x.CanSupervise).HasDefaultValue(false);
            entity.Property(x => x.CanApprove).HasDefaultValue(false);
            entity.Property(x => x.ArchiveReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UnitType, x.Name }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Code })
                .HasFilter("\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NULL")
                .IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ParentOrgUnitId, x.Code })
                .HasFilter("\"Code\" IS NOT NULL AND \"ParentOrgUnitId\" IS NOT NULL")
                .IsUnique();
            entity.HasOne(x => x.ParentOrgUnit).WithMany().HasForeignKey(x => x.ParentOrgUnitId);
            entity.HasOne(x => x.ManagerPerson).WithMany().HasForeignKey(x => x.ManagerPersonId);
            entity.HasOne(x => x.DefaultSiteOrgUnit).WithMany().HasForeignKey(x => x.DefaultSiteOrgUnitId);
            entity.HasOne(x => x.ArchivedByUser).WithMany().HasForeignKey(x => x.ArchivedByUserId);
        });

        modelBuilder.Entity<InternalLocation>(entity =>
        {
            entity.ToTable("staffarr_internal_locations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LocationNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.LocationType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AllowedProductUsage).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ArchiveReason).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SiteOrgUnitId);
            entity.HasIndex(x => x.ParentLocationId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.TenantId, x.LocationNumber })
                .HasFilter("\"ParentLocationId\" IS NULL")
                .IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ParentLocationId, x.LocationNumber })
                .HasFilter("\"ParentLocationId\" IS NOT NULL")
                .IsUnique();
            entity.HasOne(x => x.ParentLocation).WithMany().HasForeignKey(x => x.ParentLocationId);
            entity.HasOne(x => x.SiteOrgUnit).WithMany().HasForeignKey(x => x.SiteOrgUnitId);
            entity.HasOne(x => x.ArchivedByUser).WithMany().HasForeignKey(x => x.ArchivedByUserId);
        });

        modelBuilder.Entity<StaffPerson>(entity =>
        {
            entity.ToTable("staffarr_people");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.GivenName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FamilyName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LegalFirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LegalMiddleName).HasMaxLength(100);
            entity.Property(x => x.LegalLastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PreferredName).HasMaxLength(100);
            entity.Property(x => x.Pronouns).HasMaxLength(64);
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PrimaryEmail).HasMaxLength(320).IsRequired();
            entity.Property(x => x.AlternateEmail).HasMaxLength(320);
            entity.Property(x => x.PrimaryPhone).HasMaxLength(32);
            entity.Property(x => x.AlternatePhone).HasMaxLength(32);
            entity.Property(x => x.EmploymentStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WorkRelationshipType).HasMaxLength(32);
            entity.Property(x => x.EmploymentType).HasMaxLength(32);
            entity.Property(x => x.JobTitle).HasMaxLength(128);
            entity.Property(x => x.WorkPhone).HasMaxLength(32);
            entity.Property(x => x.StartDate);
            entity.Property(x => x.ExpectedStartDate);
            entity.Property(x => x.CanLoginSnapshot).HasDefaultValue(false);
            entity.Property(x => x.HasUserAccountSnapshot).HasDefaultValue(false);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PrimaryEmail });
            entity.HasIndex(x => new { x.TenantId, x.ExternalUserId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ManagerPersonId });
            entity.HasOne(x => x.PrimaryOrgUnit).WithMany().HasForeignKey(x => x.PrimaryOrgUnitId);
            entity.HasOne(x => x.HomeBaseLocation).WithMany().HasForeignKey(x => x.HomeBaseLocationId);
            entity.HasOne(x => x.Manager).WithMany().HasForeignKey(x => x.ManagerPersonId);
        });

        modelBuilder.Entity<OrgUnitAssignment>(entity =>
        {
            entity.ToTable("staffarr_org_unit_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(256);
            entity.Property(x => x.IsPrimary).HasDefaultValue(false);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.PersonId,
                x.SiteOrgUnitId,
                x.DepartmentOrgUnitId,
                x.TeamOrgUnitId,
                x.PositionOrgUnitId
            })
            .HasFilter("\"Status\" IN ('planned','active')")
            .IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PersonId })
                .HasFilter("\"IsPrimary\" = TRUE AND \"Status\" IN ('planned','active')")
                .IsUnique();
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
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PermissionScope).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Sensitivity).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PermissionKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProductKey, x.PermissionKey }).IsUnique();
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
            entity.Property(x => x.ExpiresAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status, x.ExpiresAt });
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

        modelBuilder.Entity<StaffRole>(entity =>
        {
            entity.ToTable("staffarr_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.RoleType).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Name });
            entity.HasIndex(x => new { x.TenantId, x.IsArchived });
        });

        modelBuilder.Entity<StaffRolePermission>(entity =>
        {
            entity.ToTable("staffarr_role_permissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PermissionKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Effect).HasMaxLength(16).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoleId });
            entity.HasIndex(x => new { x.TenantId, x.RoleId, x.ProductKey, x.PermissionKey, x.Effect }).IsUnique();
        });

        modelBuilder.Entity<StaffRoleScope>(entity =>
        {
            entity.ToTable("staffarr_role_scopes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScopeRefId).HasMaxLength(128);
            entity.Property(x => x.ScopeRefSnapshot).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoleId });
            entity.HasIndex(x => new { x.TenantId, x.RoleId, x.ScopeType, x.ScopeRefId }).IsUnique();
        });

        modelBuilder.Entity<StaffPersonRole>(entity =>
        {
            entity.ToTable("staffarr_person_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssignmentScopeType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AssignmentScopeRefId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId });
            entity.HasIndex(x => new { x.TenantId, x.RoleId });
            entity.HasIndex(x => new
            {
                x.TenantId,
                x.PersonId,
                x.RoleId,
                x.AssignmentScopeType,
                x.AssignmentScopeRefId
            }).IsUnique();
        });

        modelBuilder.Entity<PermissionCatalogCacheEntry>(entity =>
        {
            entity.ToTable("staffarr_permission_catalog_cache");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CatalogVersion).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CatalogJson).HasColumnType("text").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProductKey, x.IsActive });
            entity.HasIndex(x => new { x.TenantId, x.ProductKey, x.CatalogVersion }).IsUnique();
        });

        modelBuilder.Entity<PermissionAuditLogEntry>(entity =>
        {
            entity.ToTable("staffarr_permission_audit_log");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.BeforeJson).HasColumnType("text");
            entity.Property(x => x.AfterJson).HasColumnType("text");
            entity.Property(x => x.Reason).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RoleId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.ActorPersonId, x.CreatedAt });
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

        modelBuilder.Entity<PersonTrainingAcknowledgement>(entity =>
        {
            entity.ToTable("staffarr_person_training_acknowledgements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrainingTitle).HasMaxLength(256).IsRequired();
            entity.Property(x => x.AssignmentReason).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TrainarrAcknowledgementRequestId }).IsUnique();
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
            entity.Property(x => x.IncidentSource).HasMaxLength(64);
            entity.Property(x => x.IncidentType).HasMaxLength(64);
            entity.Property(x => x.LocationDetail).HasMaxLength(256);
            entity.Property(x => x.WitnessPersonIdsJson).HasMaxLength(2048);
            entity.Property(x => x.AdditionalInvolvedPersonIdsJson).HasMaxLength(2048);
            entity.Property(x => x.ImmediateActionsTaken).HasMaxLength(2000);
            entity.Property(x => x.RootCause).HasMaxLength(2000);
            entity.Property(x => x.CategoryKeysJson).HasMaxLength(1024);
            entity.Property(x => x.ReadinessDecision).HasMaxLength(32);
            entity.Property(x => x.WorkRestriction).HasMaxLength(64);
            entity.Property(x => x.ReturnToWorkNeeded).HasMaxLength(32);
            entity.Property(x => x.PpeConcern).HasMaxLength(64);
            entity.Property(x => x.MedicalAttention).HasMaxLength(64);
            entity.Property(x => x.OutOfServiceRemoveFromDuty).HasMaxLength(32);
            entity.Property(x => x.FollowUpRequired).HasMaxLength(32);
            entity.Property(x => x.TrainingReviewReason).HasMaxLength(128);
            entity.Property(x => x.RelatedAssetReference).HasMaxLength(2048);
            entity.Property(x => x.RelatedWorkOrderReference).HasMaxLength(128);
            entity.Property(x => x.RelatedRouteReference).HasMaxLength(128);
            entity.Property(x => x.RelatedSupplierReference).HasMaxLength(2048);
            entity.Property(x => x.RelatedDocumentReference).HasMaxLength(128);
            entity.Property(x => x.RelatedPolicyReference).HasMaxLength(128);
            entity.Property(x => x.SourceProduct).HasMaxLength(64);
            entity.Property(x => x.SourceEventKind).HasMaxLength(64);
            entity.Property(x => x.SourceReferenceKey).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.ReportedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ReportedAt });
            entity.HasIndex(x => new { x.TenantId, x.IncidentType, x.ReportedAt });
            entity.HasIndex(x => new { x.TenantId, x.ReadinessDecision, x.ReportedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceIncidentId })
                .IsUnique()
                .HasDatabaseName("IX_staffarr_personnel_incidents_source_incident")
                .HasFilter("\"SourceIncidentId\" IS NOT NULL");
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<IncidentNote>(entity =>
        {
            entity.ToTable("staffarr_incident_notes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NoteTypeKey).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(8192).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DueAt);
            entity.Property(x => x.CompletedAt);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IncidentId, x.Status });
            entity.HasOne<PersonnelIncident>().WithMany().HasForeignKey(x => x.IncidentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncidentAttachment>(entity =>
        {
            entity.ToTable("staffarr_incident_attachments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentId, x.CreatedAt });
            entity.HasOne<PersonnelIncident>().WithMany().HasForeignKey(x => x.IncidentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncidentSupplyDemandLine>(entity =>
        {
            entity.ToTable("staffarr_incident_supply_demand_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatusMessage).HasMaxLength(512);
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IncidentId });
            entity.HasIndex(x => new { x.TenantId, x.IncidentId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPublicationId });
            entity.HasIndex(x => new { x.TenantId, x.ProcurementStatus });
            entity.HasOne(x => x.Incident)
                .WithMany()
                .HasForeignKey(x => x.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncidentSupplyDemandStatusEvent>(entity =>
        {
            entity.ToTable("staffarr_incident_supply_demand_status_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPublicationId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrCallbackPublicationId }).IsUnique();
        });

        modelBuilder.Entity<PersonnelNote>(entity =>
        {
            entity.ToTable("staffarr_personnel_notes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CategoryKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VisibilityKey).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(8192).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.CreatedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<PersonnelDocument>(entity =>
        {
            entity.ToTable("staffarr_personnel_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.DocumentTypeKey, x.Status });
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
            entity.Property(x => x.ConfidenceLevel).HasMaxLength(16).IsRequired().HasDefaultValue("low");
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

        modelBuilder.Entity<TenantPersonExportPreset>(entity =>
        {
            entity.ToTable("staffarr_tenant_person_export_presets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmploymentStatus).HasMaxLength(32);
            entity.Property(x => x.PresetKey).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasOne(x => x.OrgUnit).WithMany().HasForeignKey(x => x.OrgUnitId);
        });

        modelBuilder.Entity<TenantPersonExportSchedule>(entity =>
        {
            entity.ToTable("staffarr_tenant_person_export_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.IsEnabled, x.LastDeliveredAt });
        });

        modelBuilder.Entity<PersonExportDeliveryNotification>(entity =>
        {
            entity.ToTable("staffarr_person_export_delivery_notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(32).IsRequired();
            entity.Property(x => x.DeliveryStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.AttemptedAt);
        });

        modelBuilder.Entity<PersonExportDeliveryRun>(entity =>
        {
            entity.ToTable("staffarr_person_export_delivery_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EmploymentStatus).HasMaxLength(32);
            entity.Property(x => x.SkipReason).HasMaxLength(256);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.StartedAt);
        });

        modelBuilder.Entity<AuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("staffarr_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.FilterJson).HasMaxLength(4096);
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.ArtifactZip);
            entity.Property(x => x.ArtifactJson);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<PersonnelHistoryRollup>(entity =>
        {
            entity.ToTable("staffarr_personnel_history_rollups");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
            entity.HasMany(x => x.Events).WithOne(x => x.Rollup).HasForeignKey(x => x.RollupId);
        });

        modelBuilder.Entity<PersonnelHistoryEvent>(entity =>
        {
            entity.ToTable("staffarr_personnel_history_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntryId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(2048);
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalReferenceId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.EntryId }).IsUnique();
            entity.HasOne<StaffPerson>().WithMany().HasForeignKey(x => x.PersonId);
        });

        modelBuilder.Entity<TenantStaffArrWorkerSettings>(entity =>
        {
            entity.ToTable("staffarr_tenant_worker_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkerKey).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.WorkerKey }).IsUnique();
            entity.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<StaffArrWorkerRun>(entity =>
        {
            entity.ToTable("staffarr_worker_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkerKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkerKey, x.StartedAt });
        });

        modelBuilder.Entity<PersonOffboardingRecord>(entity =>
        {
            entity.ToTable("staffarr_person_offboarding_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SeparationReason).HasMaxLength(512);
            entity.Property(x => x.TargetEmploymentStatus).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.StartedAt });
            entity.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            entity.HasMany(x => x.Steps).WithOne(x => x.OffboardingRecord).HasForeignKey(x => x.OffboardingRecordId);
        });

        modelBuilder.Entity<PersonOffboardingStep>(entity =>
        {
            entity.ToTable("staffarr_person_offboarding_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StepKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BlockerDetail).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OffboardingRecordId, x.StepKey }).IsUnique();
        });

        modelBuilder.Entity<PersonnelUpdateRequest>(entity =>
        {
            entity.ToTable("staffarr_personnel_update_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.FieldKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CurrentValue).HasMaxLength(512);
            entity.Property(x => x.RequestedValue).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(2048);
            entity.Property(x => x.ReviewNotes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.SubmittedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.SubmittedAt });
            entity.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
        });
    }
}

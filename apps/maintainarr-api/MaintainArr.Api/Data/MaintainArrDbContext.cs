using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Data;

public sealed class MaintainArrDbContext(DbContextOptions<MaintainArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<AssetClass> AssetClasses => Set<AssetClass>();

    public DbSet<AssetType> AssetTypes => Set<AssetType>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<MaintainArrAuditEvent> AuditEvents => Set<MaintainArrAuditEvent>();

    public DbSet<PmSchedule> PmSchedules => Set<PmSchedule>();

    public DbSet<PmProgram> PmPrograms => Set<PmProgram>();

    public DbSet<PmProgramSchedule> PmProgramSchedules => Set<PmProgramSchedule>();

    public DbSet<InspectionTemplate> InspectionTemplates => Set<InspectionTemplate>();

    public DbSet<InspectionTemplateCategory> InspectionTemplateCategories => Set<InspectionTemplateCategory>();

    public DbSet<InspectionChecklistItem> InspectionChecklistItems => Set<InspectionChecklistItem>();

    public DbSet<InspectionTemplateAssetType> InspectionTemplateAssetTypes => Set<InspectionTemplateAssetType>();

    public DbSet<InspectionRun> InspectionRuns => Set<InspectionRun>();

    public DbSet<InspectionRunAnswer> InspectionRunAnswers => Set<InspectionRunAnswer>();

    public DbSet<Defect> Defects => Set<Defect>();

    public DbSet<DefectEvidence> DefectEvidence => Set<DefectEvidence>();

    public DbSet<InspectionRunEvidence> InspectionRunEvidence => Set<InspectionRunEvidence>();

    public DbSet<AssetMeter> AssetMeters => Set<AssetMeter>();

    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<WorkOrderTaskLine> WorkOrderTaskLines => Set<WorkOrderTaskLine>();

    public DbSet<WorkOrderLaborEntry> WorkOrderLaborEntries => Set<WorkOrderLaborEntry>();

    public DbSet<WorkOrderEvidence> WorkOrderEvidence => Set<WorkOrderEvidence>();

    public DbSet<WorkOrderPartsDemandLine> WorkOrderPartsDemandLines => Set<WorkOrderPartsDemandLine>();

    public DbSet<WorkOrderPartsDemandStatusEvent> WorkOrderPartsDemandStatusEvents => Set<WorkOrderPartsDemandStatusEvent>();

    public DbSet<TenantMaintenanceNotificationSettings> TenantMaintenanceNotificationSettings =>
        Set<TenantMaintenanceNotificationSettings>();

    public DbSet<MaintenanceNotificationDispatch> MaintenanceNotificationDispatches =>
        Set<MaintenanceNotificationDispatch>();

    public DbSet<AuditPackageGenerationJob> AuditPackageGenerationJobs => Set<AuditPackageGenerationJob>();

    public DbSet<TenantDefectEscalationSettings> TenantDefectEscalationSettings =>
        Set<TenantDefectEscalationSettings>();

    public DbSet<DefectEscalationRun> DefectEscalationRuns => Set<DefectEscalationRun>();

    public DbSet<DefectEscalationEvent> DefectEscalationEvents => Set<DefectEscalationEvent>();

    public DbSet<TenantAssetStatusRollupSettings> TenantAssetStatusRollupSettings =>
        Set<TenantAssetStatusRollupSettings>();

    public DbSet<AssetStatusRollup> AssetStatusRollups => Set<AssetStatusRollup>();

    public DbSet<AssetStatusScopeRollup> AssetStatusScopeRollups => Set<AssetStatusScopeRollup>();

    public DbSet<AssetStatusRollupRun> AssetStatusRollupRuns => Set<AssetStatusRollupRun>();

    public DbSet<TenantMaintenanceHistoryRollupSettings> TenantMaintenanceHistoryRollupSettings =>
        Set<TenantMaintenanceHistoryRollupSettings>();

    public DbSet<MaintenanceHistoryRollup> MaintenanceHistoryRollups => Set<MaintenanceHistoryRollup>();

    public DbSet<MaintenanceHistoryEvent> MaintenanceHistoryEvents => Set<MaintenanceHistoryEvent>();

    public DbSet<MaintenanceHistoryRollupRun> MaintenanceHistoryRollupRuns => Set<MaintenanceHistoryRollupRun>();

    public DbSet<TenantPmDueScanSettings> TenantPmDueScanSettings => Set<TenantPmDueScanSettings>();

    public DbSet<PmDueScanRun> PmDueScanRuns => Set<PmDueScanRun>();

    public DbSet<ComplianceRegulatoryKeyMirror> ComplianceRegulatoryKeyMirrors => Set<ComplianceRegulatoryKeyMirror>();

    public DbSet<MaintainArrImportBatch> MaintainArrImportBatches => Set<MaintainArrImportBatch>();

    public DbSet<MaintainArrStaffPersonRef> StaffPersonRefs => Set<MaintainArrStaffPersonRef>();

    public DbSet<TenantDowntimeTrackingSettings> TenantDowntimeTrackingSettings =>
        Set<TenantDowntimeTrackingSettings>();

    public DbSet<AssetDowntimeEvent> AssetDowntimeEvents => Set<AssetDowntimeEvent>();

    public DbSet<AssetAvailabilitySnapshot> AssetAvailabilitySnapshots => Set<AssetAvailabilitySnapshot>();

    public DbSet<FleetAvailabilitySnapshot> FleetAvailabilitySnapshots => Set<FleetAvailabilitySnapshot>();

    public DbSet<AssetDowntimeSyncRun> AssetDowntimeSyncRuns => Set<AssetDowntimeSyncRun>();

    public DbSet<TenantMaintenancePlatformEventSettings> TenantMaintenancePlatformEventSettings =>
        Set<TenantMaintenancePlatformEventSettings>();

    public DbSet<MaintenancePlatformOutboxEvent> MaintenancePlatformOutboxEvents =>
        Set<MaintenancePlatformOutboxEvent>();

    public DbSet<MaintenancePlatformEventProcessingRun> MaintenancePlatformEventProcessingRuns =>
        Set<MaintenancePlatformEventProcessingRun>();

    public DbSet<MaintenanceInboundPlatformEvent> MaintenanceInboundPlatformEvents =>
        Set<MaintenanceInboundPlatformEvent>();

    public DbSet<CatalogDefinition> CatalogDefinitions => Set<CatalogDefinition>();
    public DbSet<CatalogOption> CatalogOptions => Set<CatalogOption>();
    public DbSet<CatalogOptionDependency> CatalogOptionDependencies => Set<CatalogOptionDependency>();
    public DbSet<FieldsetDefinition> FieldsetDefinitions => Set<FieldsetDefinition>();
    public DbSet<FieldsetField> FieldsetFields => Set<FieldsetField>();
    public DbSet<PendingCatalogValue> PendingCatalogValues => Set<PendingCatalogValue>();
    public DbSet<ReferenceCacheEntry> ReferenceCacheEntries => Set<ReferenceCacheEntry>();
    public DbSet<AssetCustomFieldValue> AssetCustomFieldValues => Set<AssetCustomFieldValue>();
    public DbSet<AssetSpec> AssetSpecs => Set<AssetSpec>();
    public DbSet<AssetComponent> AssetComponents => Set<AssetComponent>();
    public DbSet<AssetDocumentRef> AssetDocumentRefs => Set<AssetDocumentRef>();
    public DbSet<AssetComplianceState> AssetComplianceStates => Set<AssetComplianceState>();
    public DbSet<AssetStatusHistory> AssetStatusHistory => Set<AssetStatusHistory>();
    public DbSet<AssetLocationHistory> AssetLocationHistory => Set<AssetLocationHistory>();
    public DbSet<AssetAssignmentHistory> AssetAssignmentHistory => Set<AssetAssignmentHistory>();
    public DbSet<AssetReadinessState> AssetReadinessStates => Set<AssetReadinessState>();
    public DbSet<AssetExternalMapping> AssetExternalMappings => Set<AssetExternalMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AssetClass>(entity =>
        {
            entity.ToTable("maintainarr_asset_classes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClassKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ClassKey }).IsUnique();
        });

        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.ToTable("maintainarr_asset_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TypeKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TypeKey }).IsUnique();
            entity.HasOne(x => x.AssetClass)
                .WithMany()
                .HasForeignKey(x => x.AssetClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("maintainarr_assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetTag).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.LifecycleStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SiteRef).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetTag }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AssetTypeId });
            entity.HasOne(x => x.AssetType)
                .WithMany()
                .HasForeignKey(x => x.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaintainArrAuditEvent>(entity =>
        {
            entity.ToTable("maintainarr_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TargetType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetId).HasMaxLength(128);
            entity.Property(x => x.Result).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(64);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OccurredAt });
        });

        modelBuilder.Entity<PmProgram>(entity =>
        {
            entity.ToTable("maintainarr_pm_programs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProgramKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProgramKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.AssetTypeId });
            entity.HasIndex(x => new { x.TenantId, x.AssetId });
            entity.HasOne(x => x.AssetType)
                .WithMany()
                .HasForeignKey(x => x.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PmProgramSchedule>(entity =>
        {
            entity.ToTable("maintainarr_pm_program_schedules");
            entity.HasKey(x => new { x.PmProgramId, x.PmScheduleId });
            entity.HasIndex(x => x.PmScheduleId).IsUnique();
            entity.HasIndex(x => new { x.PmProgramId, x.SortOrder });
            entity.HasOne(x => x.PmProgram)
                .WithMany(x => x.ProgramSchedules)
                .HasForeignKey(x => x.PmProgramId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PmSchedule)
                .WithMany()
                .HasForeignKey(x => x.PmScheduleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PmSchedule>(entity =>
        {
            entity.ToTable("maintainarr_pm_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScheduleKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.ScheduleMode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.IntervalUsage).HasPrecision(18, 4);
            entity.Property(x => x.NextDueAtUsage).HasPrecision(18, 4);
            entity.Property(x => x.LastCompletedUsage).HasPrecision(18, 4);
            entity.Property(x => x.DueStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.ScheduleKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.DueStatus, x.NextDueAt });
            entity.HasIndex(x => new { x.TenantId, x.AssetMeterId, x.Status });
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AssetMeter)
                .WithMany()
                .HasForeignKey(x => x.AssetMeterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AssetMeter>(entity =>
        {
            entity.ToTable("maintainarr_asset_meters");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MeterKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Unit).HasMaxLength(32).IsRequired();
            entity.Property(x => x.BaselineReading).HasPrecision(18, 4);
            entity.Property(x => x.CurrentReading).HasPrecision(18, 4);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.MeterKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.Status });
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.ToTable("maintainarr_meter_readings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReadingValue).HasPrecision(18, 4);
            entity.Property(x => x.DeltaFromPrevious).HasPrecision(18, 4);
            entity.Property(x => x.Notes).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetMeterId, x.ReadAt });
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.ReadAt });
            entity.HasOne(x => x.AssetMeter)
                .WithMany(x => x.Readings)
                .HasForeignKey(x => x.AssetMeterId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InspectionTemplate>(entity =>
        {
            entity.ToTable("maintainarr_inspection_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TemplateKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(512);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TemplateKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status });
        });

        modelBuilder.Entity<InspectionTemplateCategory>(entity =>
        {
            entity.ToTable("maintainarr_inspection_template_categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CategoryKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InspectionTemplateId, x.CategoryKey }).IsUnique();
            entity.HasOne(x => x.InspectionTemplate)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.InspectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InspectionChecklistItem>(entity =>
        {
            entity.ToTable("maintainarr_inspection_checklist_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ItemKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Prompt).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ItemType).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InspectionTemplateId, x.ItemKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.InspectionTemplateId, x.SortOrder });
            entity.HasOne(x => x.InspectionTemplate)
                .WithMany(x => x.ChecklistItems)
                .HasForeignKey(x => x.InspectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InspectionTemplateAssetType>(entity =>
        {
            entity.ToTable("maintainarr_inspection_template_asset_types");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InspectionTemplateId, x.AssetTypeId }).IsUnique();
            entity.HasOne(x => x.InspectionTemplate)
                .WithMany(x => x.AssetTypeLinks)
                .HasForeignKey(x => x.InspectionTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.AssetType)
                .WithMany()
                .HasForeignKey(x => x.AssetTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InspectionRun>(entity =>
        {
            entity.ToTable("maintainarr_inspection_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(32);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.InspectionTemplateId });
            entity.HasIndex(x => new { x.TenantId, x.StartedByUserId, x.StartedAt });
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InspectionTemplate)
                .WithMany()
                .HasForeignKey(x => x.InspectionTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InspectionRunAnswer>(entity =>
        {
            entity.ToTable("maintainarr_inspection_run_answers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PassFailValue).HasMaxLength(16);
            entity.Property(x => x.TextValue).HasMaxLength(512);
            entity.Property(x => x.NumericValue).HasPrecision(18, 4);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InspectionRunId, x.ChecklistItemId }).IsUnique();
            entity.HasOne(x => x.InspectionRun)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.InspectionRunId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChecklistItem)
                .WithMany()
                .HasForeignKey(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Defect>(entity =>
        {
            entity.ToTable("maintainarr_defects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.Severity).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.InspectionRunId });
            entity.HasIndex(x => new { x.TenantId, x.ReportedByUserId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UpdatedAt });
            entity.HasIndex(x => new { x.TenantId, x.InspectionRunId, x.ChecklistItemId })
                .IsUnique()
                .HasFilter("\"ChecklistItemId\" IS NOT NULL");
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.InspectionRun)
                .WithMany()
                .HasForeignKey(x => x.InspectionRunId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ChecklistItem)
                .WithMany()
                .HasForeignKey(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("maintainarr_work_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkOrderNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.Priority).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AssignedTechnicianPersonId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.DefectId });
            entity.HasIndex(x => new { x.TenantId, x.PmScheduleId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.CreatedByUserId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.AssignedTechnicianPersonId, x.Status });
            entity.HasOne(x => x.Asset)
                .WithMany()
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Defect)
                .WithMany()
                .HasForeignKey(x => x.DefectId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.PmSchedule)
                .WithMany()
                .HasForeignKey(x => x.PmScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkOrderTaskLine>(entity =>
        {
            entity.ToTable("maintainarr_work_order_task_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1024);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderId, x.SortOrder });
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.TaskLines)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkOrderLaborEntry>(entity =>
        {
            entity.ToTable("maintainarr_work_order_labor_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.HoursWorked).HasPrecision(8, 2);
            entity.Property(x => x.LaborTypeKey).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderId, x.LoggedAt });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.LoggedAt });
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.LaborEntries)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.WorkOrderTaskLine)
                .WithMany()
                .HasForeignKey(x => x.WorkOrderTaskLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkOrderEvidence>(entity =>
        {
            entity.ToTable("maintainarr_work_order_evidence");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderId, x.CreatedAt });
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.Evidence)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DefectEvidence>(entity =>
        {
            entity.ToTable("maintainarr_defect_evidence");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DefectId, x.CreatedAt });
            entity.HasOne(x => x.Defect)
                .WithMany(x => x.Evidence)
                .HasForeignKey(x => x.DefectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InspectionRunEvidence>(entity =>
        {
            entity.ToTable("maintainarr_inspection_run_evidence");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceTypeKey).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.InspectionRunId, x.CreatedAt });
            entity.HasOne(x => x.InspectionRun)
                .WithMany(x => x.Evidence)
                .HasForeignKey(x => x.InspectionRunId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ChecklistItem)
                .WithMany()
                .HasForeignKey(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkOrderPartsDemandLine>(entity =>
        {
            entity.ToTable("maintainarr_work_order_parts_demand_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PartNumber).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(256).IsRequired();
            entity.Property(x => x.QuantityRequested).HasPrecision(18, 4);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 4);
            entity.Property(x => x.ProcurementStatusMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderId, x.LineNumber });
            entity.HasIndex(x => new { x.TenantId, x.WorkOrderId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrPublicationId });
            entity.HasIndex(x => new { x.TenantId, x.ProcurementStatus });
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.PartsDemandLines)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkOrderPartsDemandStatusEvent>(entity =>
        {
            entity.ToTable("maintainarr_work_order_parts_demand_status_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProcurementStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.MaintainarrPublicationId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.SupplyarrCallbackPublicationId }).IsUnique();
        });

        modelBuilder.Entity<TenantMaintenanceNotificationSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_notification_settings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationWebhookUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<MaintenanceNotificationDispatch>(entity =>
        {
            entity.ToTable("maintainarr_notification_dispatches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispatchStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.WebhookHost).HasMaxLength(256);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DispatchStatus, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.RelatedEntityType, x.RelatedEntityId });
        });

        modelBuilder.Entity<AuditPackageGenerationJob>(entity =>
        {
            entity.ToTable("maintainarr_audit_package_generation_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(16).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(2000);
            entity.Property(x => x.FilterJson).HasMaxLength(4096);
            entity.Property(x => x.ArtifactZip);
            entity.Property(x => x.ArtifactJson);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<TenantDefectEscalationSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_defect_escalation_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<DefectEscalationRun>(entity =>
        {
            entity.ToTable("maintainarr_defect_escalation_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<DefectEscalationEvent>(entity =>
        {
            entity.ToTable("maintainarr_defect_escalation_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActionKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PreviousSeverity).HasMaxLength(32);
            entity.Property(x => x.NewSeverity).HasMaxLength(32);
            entity.Property(x => x.PreviousStatus).HasMaxLength(32);
            entity.Property(x => x.NewStatus).HasMaxLength(32);
            entity.HasIndex(x => new { x.TenantId, x.DefectId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantAssetStatusRollupSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_asset_status_rollup_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AssetStatusRollup>(entity =>
        {
            entity.ToTable("maintainarr_asset_status_rollups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetTag).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AssetName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.LifecycleStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReadinessStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ReadinessBasis).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PrimaryBlockerMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
        });

        modelBuilder.Entity<AssetStatusScopeRollup>(entity =>
        {
            entity.ToTable("maintainarr_asset_status_scope_rollups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScopeType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ScopeEntityKey).HasMaxLength(128);
            entity.Property(x => x.ScopeLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ReadyPercent).HasPrecision(5, 1);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ScopeType, x.ScopeEntityId, x.ScopeEntityKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ScopeType, x.ComputedAt });
        });

        modelBuilder.Entity<AssetStatusRollupRun>(entity =>
        {
            entity.ToTable("maintainarr_asset_status_rollup_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantMaintenanceHistoryRollupSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_maintenance_history_rollup_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<MaintenanceHistoryRollup>(entity =>
        {
            entity.ToTable("maintainarr_maintenance_history_rollups");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetTag).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AssetName).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ComputedAt });
        });

        modelBuilder.Entity<MaintenanceHistoryEvent>(entity =>
        {
            entity.ToTable("maintainarr_maintenance_history_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntryId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Detail).HasMaxLength(1024);
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityId).HasMaxLength(64);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.RollupId });
            entity.HasOne(x => x.Rollup)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.RollupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaintenanceHistoryRollupRun>(entity =>
        {
            entity.ToTable("maintainarr_maintenance_history_rollup_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantPmDueScanSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_pm_due_scan_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<PmDueScanRun>(entity =>
        {
            entity.ToTable("maintainarr_pm_due_scan_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<MaintainArrImportBatch>(entity =>
        {
            entity.ToTable("maintainarr_import_batches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImportType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Phase).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.ImportType, x.CreatedAt });
        });

        modelBuilder.Entity<ComplianceRegulatoryKeyMirror>(entity =>
        {
            entity.ToTable("maintainarr_compliance_regulatory_key_mirrors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SubjectType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ComplianceKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MaterialKey).HasMaxLength(128);
            entity.Property(x => x.RegulatoryCitationKey).HasMaxLength(128);
            entity.Property(x => x.SourceProduct).HasMaxLength(32).IsRequired();
            entity.Property(x => x.SourceRecordKey).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ComplianceKey });
            entity.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId, x.ComplianceKey }).IsUnique();
        });

        modelBuilder.Entity<MaintainArrStaffPersonRef>(entity =>
        {
            entity.ToTable("maintainarr_staff_person_refs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StaffarrPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DisplayNameSnapshot).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ActiveStatusSnapshot).HasMaxLength(64);
            entity.Property(x => x.PrimarySiteSnapshot).HasMaxLength(128);
            entity.Property(x => x.SourceCorrelationId).HasMaxLength(128);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.StaffarrPersonId }).IsUnique();
        });

        modelBuilder.Entity<TenantDowntimeTrackingSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_downtime_tracking_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AssetDowntimeEvent>(entity =>
        {
            entity.ToTable("maintainarr_asset_downtime_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetTag).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AssetName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusTrigger).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.StartedAt });
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.EndedAt });
            entity.HasIndex(x => new { x.TenantId, x.Source, x.EndedAt });
        });

        modelBuilder.Entity<AssetAvailabilitySnapshot>(entity =>
        {
            entity.ToTable("maintainarr_asset_availability_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssetTag).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AssetName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TotalHours).HasPrecision(18, 2);
            entity.Property(x => x.DowntimeHours).HasPrecision(18, 2);
            entity.Property(x => x.AvailabilityPercent).HasPrecision(5, 1);
            entity.Property(x => x.PlannedDowntimeHours).HasPrecision(18, 2);
            entity.Property(x => x.UnplannedDowntimeHours).HasPrecision(18, 2);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AssetId }).IsUnique();
        });

        modelBuilder.Entity<FleetAvailabilitySnapshot>(entity =>
        {
            entity.ToTable("maintainarr_fleet_availability_snapshots");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TotalHours).HasPrecision(18, 2);
            entity.Property(x => x.DowntimeHours).HasPrecision(18, 2);
            entity.Property(x => x.AvailabilityPercent).HasPrecision(5, 1);
            entity.Property(x => x.PlannedDowntimeHours).HasPrecision(18, 2);
            entity.Property(x => x.UnplannedDowntimeHours).HasPrecision(18, 2);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<AssetDowntimeSyncRun>(entity =>
        {
            entity.ToTable("maintainarr_asset_downtime_sync_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<TenantMaintenancePlatformEventSettings>(entity =>
        {
            entity.ToTable("maintainarr_tenant_platform_event_settings");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId).IsUnique();
        });

        modelBuilder.Entity<MaintenancePlatformOutboxEvent>(entity =>
        {
            entity.ToTable("maintainarr_platform_outbox_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(32).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProcessingStatus, x.NextRetryAt });
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.CreatedAt });
        });

        modelBuilder.Entity<MaintenancePlatformEventProcessingRun>(entity =>
        {
            entity.ToTable("maintainarr_platform_event_processing_runs");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });
        });

        modelBuilder.Entity<MaintenanceInboundPlatformEvent>(entity =>
        {
            entity.ToTable("maintainarr_inbound_platform_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EventKind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RelatedEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceEventId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.EventKind, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.CreatedDefectId });
            entity.HasOne(x => x.CreatedDefect)
                .WithMany()
                .HasForeignKey(x => x.CreatedDefectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CatalogDefinition>(entity =>
        {
            entity.ToTable("maintainarr_catalogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Owner).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Scope, x.Key }).IsUnique();
        });

        modelBuilder.Entity<CatalogOption>(entity =>
        {
            entity.ToTable("maintainarr_catalog_options");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.CatalogId, x.Key, x.OptionTenantId }).IsUnique();
        });

        modelBuilder.Entity<CatalogOptionDependency>(entity =>
        {
            entity.ToTable("maintainarr_catalog_option_dependencies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DependsOnCatalogKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DependsOnOptionKey).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<FieldsetDefinition>(entity =>
        {
            entity.ToTable("maintainarr_fieldset_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Key, x.Purpose }).IsUnique();
        });

        modelBuilder.Entity<FieldsetField>(entity =>
        {
            entity.ToTable("maintainarr_fieldset_fields");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DataType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ControlType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceOfTruth).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.FieldsetId, x.Key }).IsUnique();
        });

        modelBuilder.Entity<PendingCatalogValue>(entity =>
        {
            entity.ToTable("maintainarr_pending_catalog_values");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CatalogKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProposedKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProposedLabel).HasMaxLength(256).IsRequired();
            entity.Property(x => x.ProposedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceEntityId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<ReferenceCacheEntry>(entity =>
        {
            entity.ToTable("maintainarr_reference_cache_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceOfTruth).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReferenceKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalId).HasMaxLength(128);
            entity.Property(x => x.Label).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.SourceOfTruth, x.ReferenceKey, x.ExternalKey }).IsUnique();
        });

        modelBuilder.Entity<AssetCustomFieldValue>(entity =>
        {
            entity.ToTable("maintainarr_asset_custom_field_values");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FieldKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.FieldKey }).IsUnique();
        });

        modelBuilder.Entity<AssetSpec>(entity =>
        {
            entity.ToTable("maintainarr_asset_specs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SpecKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.SpecKey }).IsUnique();
        });

        modelBuilder.Entity<AssetComponent>(entity =>
        {
            entity.ToTable("maintainarr_asset_components");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ComponentKey).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.ComponentKey }).IsUnique();
        });

        modelBuilder.Entity<AssetDocumentRef>(entity =>
        {
            entity.ToTable("maintainarr_asset_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentTypeKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalDocumentId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StatusKey).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<AssetComplianceState>(entity =>
        {
            entity.ToTable("maintainarr_asset_compliance_state");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.TenantId, x.AssetId }).IsUnique();
        });

        modelBuilder.Entity<AssetStatusHistory>(entity =>
        {
            entity.ToTable("maintainarr_asset_status_history");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StatusFieldKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StatusValueKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChangedByPersonId).HasMaxLength(128);
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.ChangedAt });
        });

        modelBuilder.Entity<AssetLocationHistory>(entity =>
        {
            entity.ToTable("maintainarr_asset_location_history");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SiteId).HasMaxLength(128);
            entity.Property(x => x.HomeLocationId).HasMaxLength(128);
            entity.Property(x => x.CurrentLocationId).HasMaxLength(128);
            entity.Property(x => x.Yard).HasMaxLength(128);
            entity.Property(x => x.Bay).HasMaxLength(128);
            entity.Property(x => x.ParkingSpot).HasMaxLength(128);
            entity.Property(x => x.ChangedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.EffectiveAt });
        });

        modelBuilder.Entity<AssetAssignmentHistory>(entity =>
        {
            entity.ToTable("maintainarr_asset_assignment_history");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AssignmentFieldKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ChangedByPersonId).HasMaxLength(128);
            entity.HasIndex(x => new { x.TenantId, x.AssetId, x.AssignmentFieldKey, x.EffectiveAt });
        });

        modelBuilder.Entity<AssetReadinessState>(entity =>
        {
            entity.ToTable("maintainarr_asset_readiness_state");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReadinessStatusKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.OperationalStatusKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AvailabilityStatusKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Basis).HasMaxLength(256);
            entity.Property(x => x.Notes).HasMaxLength(1024);
            entity.HasIndex(x => new { x.TenantId, x.AssetId }).IsUnique();
        });

        modelBuilder.Entity<AssetExternalMapping>(entity =>
        {
            entity.ToTable("maintainarr_asset_external_mappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceSystem).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExternalEntityType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExternalKey).HasMaxLength(128);
            entity.Property(x => x.MetadataJson).HasMaxLength(4096);
            entity.HasIndex(x => new { x.TenantId, x.SourceSystem, x.ExternalEntityType, x.ExternalId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.AssetId });
        });
    }
}

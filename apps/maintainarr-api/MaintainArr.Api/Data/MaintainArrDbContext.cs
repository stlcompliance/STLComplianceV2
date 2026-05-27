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

    public DbSet<AssetMeter> AssetMeters => Set<AssetMeter>();

    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    public DbSet<WorkOrderTaskLine> WorkOrderTaskLines => Set<WorkOrderTaskLine>();

    public DbSet<WorkOrderLaborEntry> WorkOrderLaborEntries => Set<WorkOrderLaborEntry>();

    public DbSet<WorkOrderEvidence> WorkOrderEvidence => Set<WorkOrderEvidence>();

    public DbSet<WorkOrderPartsDemandLine> WorkOrderPartsDemandLines => Set<WorkOrderPartsDemandLine>();

    public DbSet<WorkOrderPartsDemandStatusEvent> WorkOrderPartsDemandStatusEvents => Set<WorkOrderPartsDemandStatusEvent>();

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
    }
}
